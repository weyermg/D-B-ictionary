using System.ComponentModel.DataAnnotations;

namespace D_B_ictionary.Api;

/// <summary>Request payload for adding a dictionary word.</summary>
public sealed record CreateWordRequest
{
    [Required, StringLength(200)]
    public required string Word { get; init; }

    [Required, StringLength(32)]
    public required string Language { get; init; }

    [Range(0, int.MaxValue)]
    public required int EtymologyIndex { get; init; }

    [Range(0, int.MaxValue)]
    public required int DefinitionIndex { get; init; }

    [Required, MinLength(1)]
    public required string[] Definitions { get; init; }
}

/// <summary>Dictionary word data returned by the API.</summary>
public sealed record WordResponse(
    string Id,
    string Word,
    string Language,
    int EtymologyIndex,
    int DefinitionIndex,
    string[] Definitions);