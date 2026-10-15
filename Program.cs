using System;
using System.Threading.Tasks;
namespace Interaction
{
    class Program
    {
        static async Task Main()
        {
            //Ingester.Parser.Skim();
            ElasticSearch.ElasticSearchConnection.Connect();
            var elastic = ElasticSearch.ElasticSearchConnection.Client;

            var deleteIndexResponse = await elastic.Indices.DeleteAsync(
                "dictionary-words",
                request => request.IgnoreUnavailable(true)
            );
            if (!deleteIndexResponse.IsValidResponse)
            {
                Console.WriteLine(deleteIndexResponse.DebugInformation);
                return;
            }

            var createIndexResponse = await elastic.Indices.CreateAsync("dictionary-words");
            if (!createIndexResponse.IsValidResponse)
            {
                Console.WriteLine(createIndexResponse.DebugInformation);
                return;
            }

            var document = new ElasticSearchWord
            {
                Word = "Cat",
                Language = "en",
                etym_index = 1,
                def_index = 1,
                Definition = "a furry animal"
            };
            var indexResponse = await elastic.IndexAsync(document, request => request
                .Refresh(Elastic.Clients.Elasticsearch.Refresh.WaitFor)
            );

            if (!indexResponse.IsValidResponse)
            {
                Console.WriteLine(indexResponse.DebugInformation);
                return;
            }
            Console.WriteLine("Document indexed successfully.");

            var searchTerm = "furr";
            var language = "en";
            var searchResponse = await elastic.SearchAsync<ElasticSearchWord>(search => search
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
                return;
            }

            var source = searchResponse.Hits.FirstOrDefault()?.Source;
            if (source is null)
            {
                Console.WriteLine("No matching word was found.");
                return;
            }

            Console.WriteLine("Document found.");
            Console.WriteLine($"Word: {source.Word}");
            Console.WriteLine($"Language: {source.Language}");
            Console.WriteLine($"Etym Index: {source.etym_index}");
            Console.WriteLine($"Def Index: {source.def_index}");
            Console.WriteLine($"Definition: {source.Definition}");
        }
    }
}