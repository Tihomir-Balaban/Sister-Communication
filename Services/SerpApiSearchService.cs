using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Sister_Communication.DTOs;
using Sister_Communication.Options;
using Sister_Communication.Services.Interfaces;

namespace Sister_Communication.Services;

public sealed class SerpApiSearchService(HttpClient http, IOptions<SerpApiOptions> options) : ISerpApiSearchService
{
    private readonly HttpClient _http = http;
    private readonly SerpApiOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Executes a search query using the Serp API and retrieves organic search results.
    /// </summary>
    /// <param name="query">The search query string. Must not be null or empty.</param>
    /// <param name="maxResults">The maximum number of results to retrieve. Defaults to 100 and must be a positive value.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation. Defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>
    /// A read-only list of <see cref="SerpApiOrganicResultDto"/> objects representing the organic search results.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the search query is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the API key is not configured.</exception>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
    /// <exception cref="JsonException">Thrown if the response content cannot be deserialized.</exception>
    public async Task<IReadOnlyList<SerpApiOrganicResultDto>> SearchAsync(
        string query, 
        int maxResults = 100, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query must not be empty.", nameof(query));

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Serp API configuration missing. Set Serp:ApiKey.");
        
        var results = new List<SerpApiOrganicResultDto>(capacity: Math.Min(maxResults, 100));

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        for (var start = 0; results.Count < maxResults && start <= 91; start += 10)
        {   
            var remaining = maxResults - results.Count;
            var num = Math.Min(10, remaining);

            var url =
                $"search.json?engine=google" + 
                $"&q={Uri.EscapeDataString(query)}" +
                $"&num={num}&" +
                $"&start={start}" +
                $"&api_key={Uri.EscapeDataString(_options.ApiKey)}";

            using var response = await _http.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    "Serp API returned 403 Forbidden. This is usually quota, billing, or API key restriction. " +
                    $"Response: {body}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Serp API request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {body}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var data = await JsonSerializer.DeserializeAsync<SerpSearchResponseDto>(stream, JsonOptions, cancellationToken);

            var items = data?.OrganicResults;
            if (items == null || items.Count == 0)
                break;

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Link))
                    continue;
                
                if (!seen.Add(item.Link))
                    continue;
                
                if (results.Count >= maxResults)
                    break;
                
                results.Add(new SerpApiOrganicResultDto()
                {
                    Position = item.Position,
                    Link = item.Link,
                    Title = item.Title,
                    Snippet = item.Snippet,
                });
            }
            
            if (items.Count < num)
                break;
        }

        return results;
    }
}