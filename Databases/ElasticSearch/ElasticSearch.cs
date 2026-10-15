using Elastic.Clients.Elasticsearch;
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
        var settings = new ElasticsearchClientSettings(pool).DefaultIndex(IndexName);
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

    public async Task<ElasticSearchWord?> GetWord(string id, CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync<ElasticSearchWord>(id, cancellationToken);
        return response.Found ? response.Source : null;
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
            .Size(20)
            .Query(query => query
                .Bool(boolean => boolean
                    .Must(must => must
                        .MultiMatch(multiMatch => multiMatch
                            .Fields(new[] { "word^3", "definition" })
                            .Query(searchTerm)
                            .Fuzziness("AUTO")
                        )
                    )
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