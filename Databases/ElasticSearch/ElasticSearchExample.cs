namespace D_B_ictionary;

public static class ElasticSearchExample
{
    public static async Task RunAsync()
    {
        var connection = new ElasticSearch.ElasticSearchConnection();
        var document = new ElasticSearchWord
        {
            ID = 1,
            Word = "Cat",
            Language = "en",
            Etyms =
            [
                new ElasticSearchWord.Etymology
                {
                    Definition = ["a furry animal"]
                }
            ]
        };

        await connection.AddWord(document);

        var results = await connection.FuzzyFindWords("furr", "en");
        if (results.Count == 0)
        {
            Console.WriteLine("No matching word was found.");
            return;
        }

        foreach (var result in results)
        {
            Console.WriteLine("Document found.");
            Console.WriteLine($"Word: {result.Word.Word}");
            Console.WriteLine($"Language: {result.Word.Language}");
            Console.WriteLine($"ID: {result.Word.ID}");
            foreach (var etymology in result.Word.Etyms)
            {
                Console.WriteLine($"Definitions: {string.Join(", ", etymology.Definition)}");
            }
        }
    }
}