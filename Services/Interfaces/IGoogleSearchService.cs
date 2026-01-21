using Sister_Communication.DTOs;

namespace Sister_Communication.Services.Interfaces;

public interface IGoogleSearchService
{
    /// <summary>
    /// Returns up to maxResults (typically 100) Google results for the query.
    /// </summary>
    Task<IReadOnlyList<GoogleSearchItemDto>> SearchAsync(string query,
        int maxResults = 100,
        CancellationToken cancellationToken = default);
}