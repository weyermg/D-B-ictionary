using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
namespace ElasticSearch
{

    public class ElasticSearchConnection
    {
        public ElasticsearchClient Client { get; private set; }
        public ElasticSearchConnection()
        {
            var nodes = new Uri[]
            {
                new Uri("http://10.0.0.6:9200"),
                new Uri("http://10.0.0.28:9200"),
                new Uri("http://10.0.0.109:9200")
            };

            var pool = new StaticNodePool(nodes);

            var settings = new ElasticsearchClientSettings(pool)
                .DefaultIndex("dictionary-words");

            var client = new ElasticsearchClient(settings);

            Client = client;
        }

        public async Task InitializeAsync()
        {
            var deleteIndexResponse = await Client.Indices.DeleteAsync(
                "dictionary-words",
                request => request.IgnoreUnavailable(true)
            );
            if (!deleteIndexResponse.IsValidResponse)
            {
                throw new InvalidOperationException(deleteIndexResponse.DebugInformation);
            }

            var createIndexResponse = await Client.Indices.CreateAsync("dictionary-words");
            if (!createIndexResponse.IsValidResponse)
            {
                throw new InvalidOperationException(createIndexResponse.DebugInformation);
            }

            await AddWord(new ElasticSearchWord
            {
                Word = "cat",
                Language = "en",
                etym_index = 1,
                def_index = 1,
                Definition = ["a furry animal"]
            });
        }

        public async Task AddWord(ElasticSearchWord word)
        {
            var indexResponse = await Client.IndexAsync(word, request => request
                .Refresh(Refresh.WaitFor)
            );

            if (!indexResponse.IsValidResponse)
            {
                throw new InvalidOperationException(indexResponse.DebugInformation);
            }
        }

        public async Task<List<ElasticSearchWord>> FuzzyFindWords(string searchTerm, string language)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<ElasticSearchWord>();
            }

            var searchResponse = await Client.SearchAsync<ElasticSearchWord>(search => search
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
                )
            );

            if (!searchResponse.IsValidResponse)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            var source = searchResponse.Hits
                .Select(hit => hit.Source)
                .OfType<ElasticSearchWord>()
                .ToList();
            return source;
        }
    }
}