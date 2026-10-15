using System.ComponentModel.DataAnnotations;

namespace D_B_ictionary.Api;

/// <summary>Request payload for adding a dictionary word.</summary>
public sealed record CreateWordRequest
{
    [Range(0, int.MaxValue)]
    public required int InternalId { get; init; }

    [Required, StringLength(200)]
    public required string Word { get; init; }

    [Required, StringLength(32)]
    public required string Language { get; init; }

    [Required, MinLength(1)]
    public required CreateEtymologyRequest[] Etymologies { get; init; }
}

/// <summary>Definitions grouped under one etymology.</summary>
public sealed record CreateEtymologyRequest
{
    [Required, MinLength(1)]
    public required string[] Definitions { get; init; }
}

/// <summary>An etymology and its definitions returned by the API.</summary>
public sealed record EtymologyResponse(string[] Definitions);

/// <summary>Dictionary word data returned by the API.</summary>
public sealed record WordResponse(
    string DocumentId,
    int InternalId,
    string Word,
    string Language,
    EtymologyResponse[] Etymologies);