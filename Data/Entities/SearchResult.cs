using System;
using System.ComponentModel.DataAnnotations;

namespace Sister_Communication.Data.Entities;

public sealed class SearchResult
{
    
    public int Id { get; init; }

    public string Query { get; init; } = null!;

    [Required]
    public string Url { get; init; } = null!;

    public string? Title { get; init; }
    public string? Snippet { get; init; }
    public string? DisplayLink { get; init; }

    public int Position { get; init; }

    public DateTime FetchedAtUtc { get; init; }
}