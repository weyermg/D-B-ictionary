using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using System.Threading.Tasks;

namespace ElasticSearch
{

    class ElasticSearchConnection
    {
        public static ElasticsearchClient Client { get; private set; }
        public static void Connect()
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
        }
    }
}