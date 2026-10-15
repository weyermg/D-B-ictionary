using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;

namespace ElasticSearch;

public sealed class ElasticSearchConnection
{
    private const string IndexName = "dictionary-words";

    public ElasticsearchClient Client { get; }

    public ElasticSearchConnection()
    {
        var nodeAddresses = (Environment.GetEnvironmentVariable("ELASTICSEARCH_NODES") ?? "")
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(address => new Uri(address));

        var pool = new StaticNodePool(nodeAddresses);
        var settings = new ElasticsearchClientSettings(pool)
            .DefaultIndex(IndexName)
            .RequestTimeout(TimeSpan.FromSeconds(5))
            .MaxRetryTimeout(TimeSpan.FromSeconds(10));
        Client = new ElasticsearchClient(settings);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var existsResponse = await Client.Indices.ExistsAsync(IndexName, cancellationToken);
        if (!existsResponse.Exists)
        {
            var createResponse = await Client.Indices.CreateAsync(IndexName, cancellationToken);
            if (!createResponse.IsValidResponse)
            {
                throw new InvalidOperationException(createResponse.DebugInformation);
            }
        }
    }

    public async Task<string> AddWord(
        ElasticSearchWord word,
        string? id = null,
        CancellationToken cancellationToken = default)
    {
        var response = id is null
            ? await Client.IndexAsync(word, request => request.Refresh(Refresh.WaitFor), cancellationToken)
            : await Client.IndexAsync(word, id, request => request.Refresh(Refresh.WaitFor), cancellationToken);

        if (!response.IsValidResponse || string.IsNullOrWhiteSpace(response.Id))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            throw new InvalidOperationException(response.DebugInformation);
        }

        return response.Id;
    }

    public async Task<List<(string Id, ElasticSearchWord Word)>> FuzzyFindWords(
        string searchTerm,
        string language,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        var response = await Client.SearchAsync<ElasticSearchWord>(search => search
            .Size(10)
            .Query(query => query
                .Bool(boolean => boolean
                    .Should(
                        should => should.MultiMatch(multiMatch => multiMatch
                            .Fields(new[] { "word^5", "definition" }) // matches on word exactly are most relevant
                            .Query(searchTerm)),
                        should => should.MultiMatch(multiMatch => multiMatch
                            .Fields(new[] { "word^3", "definition" }) // matches on word with fuzziness are least relevant
                            .Query(searchTerm)
                            .Fuzziness("AUTO")),
                        should => should.MultiMatch(multiMatch => multiMatch
                            .Fields(new[] { "word^4", "definition" }) // matches on word with bool prefix are moderately relevant
                            .Query(searchTerm)
                            .Type(TextQueryType.BoolPrefix))
                    )
                    .MinimumShouldMatch(1)
                    .Filter(filter => filter
                        .Term(term => term
                            .Field("language.keyword")
                            .Value(language)
                        )
                    )
                )
            ), cancellationToken);

        if (!response.IsValidResponse)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            throw new InvalidOperationException(response.DebugInformation);
        }

        return response.Hits
            .Where(hit => hit.Source is not null)
            .Select(hit => (hit.Id, hit.Source!))
            .ToList();
    }
}