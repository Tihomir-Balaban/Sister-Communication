using Sister_Communication.Data.Entities;
using Sister_Communication.DTOs;

namespace Sister_Communication.Services.Interfaces;

public interface ISearchResultStoreService
{
    Task ReplaceResultsForQueryAsync(
        string query,
        IReadOnlyList<GoogleSearchItemDto> items,
        CancellationToken cancellationToken = default);

    Task<List<SearchResult>> GetResultsForQueryAsync(
        string query,
        CancellationToken cancellationToken = default);
    
    Task<List<SearchResult>> FilterResultsAsync(
        string? query,
        string likeTerm,
        CancellationToken cancellationToken = default);
}