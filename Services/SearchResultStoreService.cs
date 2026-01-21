using Sister_Communication.Data.Entities;
using Sister_Communication.DTOs;
using Sister_Communication.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Sister_Communication.Data;

namespace Sister_Communication.Services;

public sealed class SearchResultStoreService(SisterCommunicationDbContext dbContext) : ISearchResultStoreService
{
    private readonly SisterCommunicationDbContext _dbContext = dbContext;

    /// <summary>
    /// Replaces stored search results for a given query with new results provided as input.
    /// Existing results for the query are deleted and replaced with the new results.
    /// </summary>
    /// <param name="query">The search query string to identify the results to replace. Cannot be null, empty, or whitespace.</param>
    /// <param name="items">A list of new search results containing details about the query results to store. Cannot be null.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete. Optional.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if the provided query is null, empty, or consists only of whitespace.</exception>
    public async Task ReplaceResultsForQueryAsync(string query,
        IReadOnlyList<GoogleSearchItemDto> items,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query must not be empty.", nameof(query));
        
        query = query.Trim();
        
        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        #region TODO
        // TODO: Check if I want this comment or the other not sure right now, good luck future Tihomir!
        // await _dbContext.SearchResults
        //     .Where(x => x.Query == query)
        //     .ExecuteDeleteAsync(cancellationToken);
        
        var oldRows = await _dbContext.SearchResults
            .Where(x => x.Query == query)
            .ToListAsync(cancellationToken);

        _dbContext.SearchResults.RemoveRange(oldRows);
        await _dbContext.SaveChangesAsync(cancellationToken);
        #endregion

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

    /// Retrieves the list of search results stored for a specified query.
    /// <param name="query">The query string used to search for results. This value will be trimmed of any surrounding whitespace before usage.</param>
    /// <param name="cancellationToken">Optional. A CancellationToken to observe while waiting for the task to complete.</param>
    /// <return>Returns a list of <c>SearchResult</c> objects for the specified query, ordered by their position.</return>
    public Task<List<SearchResult>> GetResultsForQueryAsync(string query, CancellationToken cancellationToken = default)
    {
        query = query.Trim();

        return _dbContext.SearchResults
            .Where(x => x.Query == query)
            .OrderBy(x => x.Position)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Filters the search results based on the specified query and URL substring.
    /// </summary>
    /// <param name="query">The search query to filter results by, or null to include results with any query.</param>
    /// <param name="likeTerm">The substring to match URLs against. This value is required and must not be empty after trimming.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="SearchResult"/> objects that match the given criteria.</returns>
    public Task<List<SearchResult>> FilterResultsAsync(string? query, string likeTerm, CancellationToken cancellationToken = default)
    {
        likeTerm = likeTerm.Trim();

        var q = _dbContext.SearchResults.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.Trim();
            q = q.Where(x => x.Query == query);
        }
        
        q = q.Where(x => x.Url.Contains(likeTerm));

        return q
            .OrderBy(x => x.Position)
            .ToListAsync(cancellationToken);
    }
}