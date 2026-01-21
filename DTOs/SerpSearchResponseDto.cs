using System.Text.Json.Serialization;

namespace Sister_Communication.DTOs;

public sealed class SerpSearchResponseDto
{
    [JsonPropertyName("organic_results")]
    public List<SerpApiOrganicResultDto>? OrganicResults  { get; init; }
}