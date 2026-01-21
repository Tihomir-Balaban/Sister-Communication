using Sister_Communication.Data.Entities;
using Sister_Communication.DTOs;
using Sister_Communication.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Sister_Communication.Data;

namespace Sister_Communication.Services;

public sealed class SearchResultStoreService(SisterCommunicationDbContext dbContext) : ISearchResultStoreService
{
    private readonly SisterCommunicationDbContext _dbContext = dbContext;

    public async Task ReplaceResultsForQueryAsync(string query,
        IReadOnlyList<GoogleSearchItemDto> items,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query must not be empty.", nameof(query));
        
        query = query.Trim();
        
        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.SearchResults
            .Where(x => x.Query == query)
            .ExecuteDeleteAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var entities = items.Select(i => new SearchResult
        {
            Query = query,
            Url = i.Link,
            Title = i.Title,
            Snippet = i.Snippet,
            DisplayLink = i.DisplayLink,
            Position = i.Position,
            FetchedAtUtc = now
        }).ToList();

        await _dbContext.SearchResults.AddRangeAsync(entities, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
    }

    public Task<List<SearchResult>> GetResultsForQueryAsync(string query, CancellationToken cancellationToken = default)
    {
        query = query.Trim();

        return _dbContext.SearchResults
            .Where(x => x.Query == query)
            .OrderBy(x => x.Position)
            .ToListAsync(cancellationToken);
    }
}