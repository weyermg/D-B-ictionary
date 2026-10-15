namespace D_B_ictionary;

public static class ElasticSearchExample
{
    public static async Task RunAsync()
    {
        var connection = new ElasticSearch.ElasticSearchConnection();
        var document = new ElasticSearchWord
        {
            Word = "Cat",
            Language = "en",
            etym_index = 1,
            def_index = 1,
            Definition = ["a furry animal"]
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
            Console.WriteLine($"Word: {result.Word}");
            Console.WriteLine($"Language: {result.Language}");
            Console.WriteLine($"Etym Index: {result.etym_index}");
            Console.WriteLine($"Def Index: {result.def_index}");
            Console.WriteLine($"Definition: {string.Join(", ", result.Definition)}");
        }
    }
}