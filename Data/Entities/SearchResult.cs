using System;
using System.ComponentModel.DataAnnotations;

namespace Sister_Communication.Data.Entities;

public class SearchResult
{
    
    public int Id { get; set; }

    // The search term entered by the user
    public string Query { get; set; } = null!;

    [Required]
    public string Url { get; set; } = null!;

    // Optional but useful (from Google API)
    public string? Title { get; set; }
    public string? Snippet { get; set; }
    public string? DisplayLink { get; set; }

    // Google ranking (1–100)
    public int Position { get; set; }

    public DateTime FetchedAtUtc { get; set; }
}