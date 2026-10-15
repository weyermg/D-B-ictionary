using System;
using System.Threading.Tasks;
namespace Interaction
{
    class Program
    {
        static async Task Main()
        {
            //Ingester.Parser.Skim();
            var connection = new ElasticSearch.ElasticSearchConnection();


            var document = new ElasticSearchWord
            {
                Word = "Cat",
                Language = "en",
                etym_index = 1,
                def_index = 1,
                Definition = ["a furry animal"]
            };

            var wordResult = await connection.FuzzyFindWord("furr", "en");
            if (wordResult is null)
            {
                Console.WriteLine("No matching word was found.");
                return;
            }

            Console.WriteLine("Document found.");
            Console.WriteLine($"Word: {wordResult.Word}");
            Console.WriteLine($"Language: {wordResult.Language}");
            Console.WriteLine($"Etym Index: {wordResult.etym_index}");
            Console.WriteLine($"Def Index: {wordResult.def_index}");
            Console.WriteLine($"Definition: {wordResult.Definition}");
        }
    }
}