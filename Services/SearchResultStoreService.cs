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

    /// Asynchronously replaces all search results for the specified query with new results. This operation removes any existing results
    /// associated with the query and inserts the provided results as replacements.
    /// <param name="query">The search query whose results are to be replaced. The query will be trimmed of any whitespace.</param>
    /// <param name="items">A collection of new search result items to store for the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ReplaceResultsForQueryAsync(
        string query,
        IReadOnlyList<SerpApiOrganicResultDto> items,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query must not be empty.", nameof(query));
        
        query = query.Trim();
        
        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

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
    public async Task<List<SearchResult>> GetResultsForQueryAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        query = query.Trim();

        return await _dbContext.SearchResults
            .Where(x => x.Query == query)
            .OrderBy(x => x.Position)
            .ToListAsync(cancellationToken);
    }

    /// Asynchronously filters search results based on a specified query and a term to match.
    /// The results are filtered using a SQL-like pattern match on fields such as query, title, URL, and snippet.
    /// <param name="query">The search query to filter results. If null or whitespace, it will not be used as a filter.</param>
    /// <param name="likeTerm">The term used to perform a pattern match in filtering results. It is trimmed of whitespace and used to construct a SQL-like pattern.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <c>SearchResult</c> objects that match the specified criteria.</returns>
    public async Task<List<SearchResult>> FilterResultsAsync(
        string? query,
        string likeTerm,
        CancellationToken cancellationToken = default)
    {
        likeTerm = likeTerm?.Trim() ?? "";
        if (likeTerm.Length == 0)
            return new List<SearchResult>();

        var pattern = $"%{likeTerm}%";

        var q = _dbContext.SearchResults.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(x => x.Query == query);

        q = q.Where(x =>
            EF.Functions.Like(x.Query ?? "", pattern) ||
            EF.Functions.Like(x.Title ?? "", pattern) ||
            EF.Functions.Like(x.Url ?? "", pattern) ||
            EF.Functions.Like(x.Snippet ?? "", pattern));

        // Optional: deterministic ordering
        return await q
            .OrderByDescending(x => x.Id)
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

        if (!exactExists) return null;
        
        var exactResults = await GetResultsForQueryAsync(query, cancellationToken);
        return (query, exactResults);
    }
}