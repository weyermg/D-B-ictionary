using System.Text.Json.Serialization;

public class ElasticSearchWord
{
    [JsonPropertyName("word")]
    public required string Word {get; set;}
    [JsonPropertyName("language")]
    public required string Language {get; set;}
    [JsonPropertyName("etym_index")]
    public required int etym_index {get; set;}
    [JsonPropertyName("def_index")]
    public required int def_index {get; set;}
    [JsonPropertyName("definition")]
    public required string Definition {get; set;}

}