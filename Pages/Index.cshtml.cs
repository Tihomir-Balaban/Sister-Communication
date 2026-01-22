using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sister_Communication.Data;
using Sister_Communication.Data.Entities;
using Sister_Communication.Services;
using Sister_Communication.Services.Interfaces;

namespace Sister_Communication.Pages;

public sealed class IndexModel(
    ILogger<IndexModel> logger,
    ISerpApiSearchService serpApi,
    ISearchResultStoreService store) : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;
    private readonly ISearchResultStoreService _store = store;
    
    [BindProperty]
    public string? SearchTerm { get; set; }

    [BindProperty]
    public string? DbFilterTerm { get; set; }

    [BindProperty]
    public string? CurrentQuery { get; set; }

    public List<SearchResult> Results { get; private set; } = new();
    
    public string? StatusMessage { get; private set; }

    /// <summary>
    /// Handles the GET request for the page and performs any necessary initialization or setup.
    /// This method is called when the page is loaded via an HTTP GET request.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task OnGetAsync() => Task.CompletedTask;

    /// <summary>
    /// Handles the POST request for performing a search operation using the specified search term.
    /// Retrieves search results from a cached store or an external service and updates the page with the results.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult to render the appropriate response.</returns>
    public async Task<IActionResult> OnPostSearchAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            ModelState.AddModelError(nameof(SearchTerm), "Please enter a search term.");
            return Page();
        }

        var query = SearchTerm.Trim();
        
        var cached = await _store.TryGetCachedResultsAsync(query, cancellationToken);
        if (cached is not null)
        {
            CurrentQuery = cached.Value.MatchedQuery;
            Results.AddRange(cached.Value.Results);
            
            return Page();
        }
        
        var items = await serpApi.SearchAsync(query, maxResults: 100, cancellationToken);

        await _store.ReplaceResultsForQueryAsync(query, items, cancellationToken);

        CurrentQuery = query;
        var results = await _store.GetResultsForQueryAsync(query, cancellationToken);

        Results.AddRange(Results);
        return Page();
    }

    /// <summary>
    /// Filters the search results from the database using the specified filter term
    /// and updates the current search results list.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A Task that resolves to an <see cref="IActionResult"/> representing the updated page with filtered results.</returns>
    public async Task<IActionResult> OnPostDbFilterAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(DbFilterTerm))
        {
            ModelState.AddModelError(nameof(DbFilterTerm), "Please enter a filter term.");
            return Page();
        }
        
        if (string.IsNullOrWhiteSpace(CurrentQuery))
        {
            ModelState.AddModelError(string.Empty, "Please run a search first so there are results to filter.");
            return Page();
        }
        
        Results = await _store.FilterResultsAsync(CurrentQuery, DbFilterTerm, cancellationToken);

        return Page();
    }

    /// <summary>
    /// Handles the POST request to clear the database filter term for the current query.
    /// Resets the database filter and re-fetches the full results for the current query from the store.
    /// </summary>
    /// <param name="ct">A cancellation token used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult to render the appropriate response.</returns>
    public async Task<IActionResult> OnPostClearDbFilterAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentQuery))
            return Page();

        DbFilterTerm = null;
        Results = await _store.GetResultsForQueryAsync(CurrentQuery, ct);
        return Page();
    }
}