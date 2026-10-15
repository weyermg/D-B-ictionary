using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using System.Threading.Tasks;

namespace ElasticSearch
{

    class ElasticSearchConnection
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

            var response = client.Ping();
            Console.WriteLine(response.IsValidResponse
                ? "Elasticsearch is reachable."
                : response.DebugInformation);
                
            Client = client;

            var deleteIndexResponse = client.Indices.DeleteAsync(
                "dictionary-words",
                request => request.IgnoreUnavailable(true)
            ).Result;
            if (!deleteIndexResponse.IsValidResponse)
            {
                Console.WriteLine(deleteIndexResponse.DebugInformation);
                return;
            }

            var createIndexResponse = client.Indices.CreateAsync("dictionary-words").Result;
            if (!createIndexResponse.IsValidResponse)
            {
                Console.WriteLine(createIndexResponse.DebugInformation);
                return;
            }
        }

        public async Task AddWord(ElasticSearchWord word) {
            var indexResponse = await Client.IndexAsync(word, request => request
                .Refresh(Refresh.WaitFor)
            );

            if (!indexResponse.IsValidResponse)
            {
                Console.WriteLine(indexResponse.DebugInformation);
                return;
            }
            Console.WriteLine("Document indexed successfully.");
        }

        public async Task<ElasticSearchWord?> FuzzyFindWord(string searchTerm, string language) 
        {
            var searchResponse = await Client.SearchAsync<ElasticSearchWord>(search => search
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
                Console.WriteLine(searchResponse.DebugInformation);
                return null;
            }

            var source = searchResponse.Hits.FirstOrDefault()?.Source;
            return source;
        }
    }
}