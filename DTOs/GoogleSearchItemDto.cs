using System.Text.Json.Serialization;

namespace Sister_Communication.DTOs;

public sealed class GoogleSearchItemDto
{
    [JsonPropertyName("link")]
    public string? Link { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("snippet")]
    public string? Snippet { get; init; }

    [JsonPropertyName("displayLink")]
    public string? DisplayLink { get; init; }
}