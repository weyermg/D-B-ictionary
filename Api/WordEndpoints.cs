using System.ComponentModel.DataAnnotations;
using ElasticSearch;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace D_B_ictionary.Api;

public static class WordEndpoints
{
    public static RouteGroupBuilder MapWordEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/words").WithTags("Words");

        group.MapGet("/", async Task<Results<Ok<IReadOnlyList<WordResponse>>, BadRequest<ProblemDetails>, ProblemHttpResult>> (
            [FromQuery] string? query,
            [FromQuery] string? language,
            ElasticSearchConnection connection,
            ILogger<ElasticSearchConnection> logger,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return TypedResults.BadRequest(new ProblemDetails
                {
                    Title = "A search query is required.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var words = await connection.FuzzyFindWords(query, language ?? "en", cancellationToken);
                IReadOnlyList<WordResponse> response = words
                    .Select(result => ToResponse(result.Id, result.Word))
                    .ToArray();
                return TypedResults.Ok(response);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Elasticsearch word search failed.");
                return TypedResults.Problem(
                    title: "Search is temporarily unavailable.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        })
        .WithName("SearchWords")
        .WithSummary("Fuzzy search dictionary words and definitions")
        .Produces<IReadOnlyList<WordResponse>>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        group.MapPost("/", async Task<Results<Created<WordResponse>, ProblemHttpResult>> (
            CreateWordRequest request,
            ElasticSearchConnection connection,
            ILogger<ElasticSearchConnection> logger,
            CancellationToken cancellationToken) =>
        {
            var word = new ElasticSearchWord
            {
                Word = request.Word.Trim(),
                Language = request.Language.Trim(),
                etym_index = request.EtymologyIndex,
                def_index = request.DefinitionIndex,
                Definition = request.Definitions
                    .Where(definition => !string.IsNullOrWhiteSpace(definition))
                    .Select(definition => definition.Trim())
                    .ToArray()
            };

            if (word.Definition.Length == 0)
            {
                return TypedResults.Problem(
                    title: "At least one definition is required.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            try
            {
                var id = await connection.AddWord(word, cancellationToken: cancellationToken);
                return TypedResults.Created($"/api/words/{Uri.EscapeDataString(id)}", ToResponse(id, word));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Adding a dictionary word failed.");
                return TypedResults.Problem(
                    title: "The word could not be added.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        })
        .WithName("CreateWord")
        .WithSummary("Add a dictionary word")
        .Produces<WordResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return group;
    }

    private static WordResponse ToResponse(string id, ElasticSearchWord word) => new(
        id,
        word.Word,
        word.Language,
        word.etym_index,
        word.def_index,
        word.Definition);
}