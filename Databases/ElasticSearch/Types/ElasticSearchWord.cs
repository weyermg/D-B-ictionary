using System.Text.Json.Serialization;

public record ElasticSearchWord
{
    [JsonPropertyName("id")]
    public required int ID {get; init;}
    [JsonPropertyName("word")]
    public required string Word {get; init;}
    [JsonPropertyName("language")]
    public required string Language {get; init;}
    [JsonPropertyName("etymologies")]
    public required Etymology[] Etyms {get; init;}

    public record Etymology
    {
        [JsonPropertyName("definitions")]
        public required string[] Definition {get; init;}
    }
}