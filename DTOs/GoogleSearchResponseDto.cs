using System.Text.Json.Serialization;

namespace Sister_Communication.DTOs;

public sealed class GoogleSearchResponseDto
{
    [JsonPropertyName("items")]
    public List<GoogleSearchItemDto>? Items { get; init; }
}