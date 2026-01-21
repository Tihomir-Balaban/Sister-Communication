using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Sister_Communication.DTOs;
using Sister_Communication.Options;
using Sister_Communication.Services.Interfaces;

namespace Sister_Communication.Services;

public sealed class GoogleSearchService(HttpClient http, IOptions<GoogleOptions> options) : IGoogleSearchService
{
    private readonly HttpClient _http = http;
    private readonly GoogleOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<GoogleSearchItemDto>> SearchAsync(
        string query, 
        int maxResults = 100, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query must not be empty.", nameof(query));

        if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.Cx))
            throw new InvalidOperationException("Google API configuration missing. Set Google:ApiKey and Google:Cx.");
        
        var results = new List<GoogleSearchItemDto>(capacity: Math.Min(maxResults, 100));
        int position = 1;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        for (var start = 1; results.Count < maxResults && start <= 91; start += 10)
        {   
            var remaining = maxResults - results.Count;
            var num = Math.Min(10, remaining);

            var url =
                $"customsearch/v1?key={Uri.EscapeDataString(_options.ApiKey)}" +
                $"&cx={Uri.EscapeDataString(_options.Cx)}" +
                $"&q={Uri.EscapeDataString(query)}" +
                $"&start={start}" +
                $"&num={num}";

            using var resp = await _http.GetAsync(url, cancellationToken);

            if (resp.StatusCode == HttpStatusCode.Forbidden)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    "Google API returned 403 Forbidden. This is usually quota, billing, or API key restriction. " +
                    $"Response: {body}");
            }

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Google API request failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Response: {body}");
            }

            await using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
            var data = await JsonSerializer.DeserializeAsync<GoogleSearchResponseDto>(stream, JsonOptions, cancellationToken);

            var items = data?.Items;
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
                
                results.Add(new GoogleSearchItemDto()
                {
                    Position = position++,
                    Link = item.Link,
                    Title = item.Title,
                    Snippet = item.Snippet,
                    DisplayLink = item.DisplayLink
                });
            }
            
            if (items.Count < num)
                break;
        }

        return results;
    }
}