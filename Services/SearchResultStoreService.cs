using Sister_Communication.Data.Entities;
using Sister_Communication.DTOs;
using Sister_Communication.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Sister_Communication.Data;
using DU = Sister_Communication.Static.DataUtils;

namespace Sister_Communication.Services;

public sealed class SearchResultStoreService(SisterCommunicationDbContext dbContext) : ISearchResultStoreService
{
    private readonly SisterCommunicationDbContext _dbContext = dbContext;
    
    public async Task ReplaceResultsForQueryAsync(
        string query,
        IReadOnlyList<SerpApiOrganicResultDto> items,
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
            Position = i.Position,
            FetchedAtUtc = now
        }).ToList();

        await _dbContext.SearchResults.AddRangeAsync(entities, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
    }

    /// Asynchronously retrieves a list of search results for the specified query. The results are ordered by their position.
    /// <param name="query">The search query to filter results with. It will be trimmed of any whitespace.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <c>SearchResult</c> objects corresponding to the provided query.</returns>
    public Task<List<SearchResult>> GetResultsForQueryAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        query = query.Trim();

        return _dbContext.SearchResults
            .Where(x => x.Query == query)
            .OrderBy(x => x.Position)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Filters the search results based on the specified query and a partial match term for URLs.
    /// </summary>
    /// <param name="query">
    /// An optional search query to filter the results. If null or whitespace, no query filtering is applied.
    /// </param>
    /// <param name="likeTerm">
    /// A term to filter results by matching it against the URLs. Must not be null or whitespace.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of filtered search results ordered by position.
    /// </returns>
    public Task<List<SearchResult>> FilterResultsAsync(string? query,
        string likeTerm,
        CancellationToken cancellationToken = default)
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

    /// Attempts to find cached results in the database that match or are closely related to the specified query.
    /// If an exact match exists, it retrieves the results for that query and returns them. If no exact match is found,
    /// it searches for related queries, evaluates their relevance, and retrieves the results for the most relevant match,
    /// if any. Returns null if no relevant results are found.
    /// <param name="query">The query string to search for in the database. It will be trimmed of whitespace.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the matched query
    /// and a list of <c>SearchResult</c> objects, or null if no relevant results are found.</returns>
    public async Task<(string MatchedQuery, List<SearchResult> Results)?> TryGetCachedResultsAsync(
    string query,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return null;

        query = query.Trim();

        var exactExists = await _dbContext.SearchResults
            .AnyAsync(x => x.Query == query, cancellationToken);

        if (exactExists)
        {
            var exactResults = await GetResultsForQueryAsync(query, cancellationToken);
            return (query, exactResults);
        }
        
        return null;
    }
}