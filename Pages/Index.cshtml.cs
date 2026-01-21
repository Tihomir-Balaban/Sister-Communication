using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sister_Communication.Data;
using Sister_Communication.Data.Entities;
using Sister_Communication.Services.Interfaces;

namespace Sister_Communication.Pages;

public sealed class IndexModel(
    ILogger<IndexModel> logger,
    IGoogleSearchService google,
    ISearchResultStoreService store) : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;
    private readonly IGoogleSearchService _google = google;
    private readonly ISearchResultStoreService _store = store;
    
    [BindProperty]
    public string? SearchTerm { get; set; }

    [BindProperty]
    public string? DbFilterTerm { get; set; }

    [BindProperty]
    public string? CurrentQuery { get; set; }

    public List<SearchResult> Results { get; private set; } = new();

    /// <summary>
    /// Handles GET requests to initialize the page with default data or state.
    /// </summary>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    public Task OnGetAsync() => Task.CompletedTask;

    /// <summary>
    /// Handles the search operation triggered by a POST request. Validates the search term,
    /// performs a Google search, stores the results, and prepares them for display.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IActionResult"/> that represents the state of the search operation.
    /// Returns the current page with updated search results, or the page with validation errors if the search term is invalid.</returns>
    public async Task<IActionResult> OnPostSearchAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            ModelState.AddModelError(nameof(SearchTerm), "Please enter a search term.");
            return Page();
        }

        var query = SearchTerm.Trim();
        
        var items = await _google.SearchAsync(query, maxResults: 100, cancellationToken);

        await _store.ReplaceResultsForQueryAsync(query, items, cancellationToken);

        CurrentQuery = query;
        Results = await _store.GetResultsForQueryAsync(query, cancellationToken);

        return Page();
    }

    /// <summary>
    /// Filters previously stored search results based on the provided filter term
    /// and updates the filtered results for the current query.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used by other objects or threads to receive
    /// notice of cancellation.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that re-renders the current page with the filtered results.
    /// </returns>
    public async Task<IActionResult> OnPostDbFilterAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(DbFilterTerm))
        {
            ModelState.AddModelError(nameof(DbFilterTerm), "Please enter a filter term.");
            return Page();
        }

        Results = await _store.FilterResultsAsync(CurrentQuery, DbFilterTerm, cancellationToken);

        return Page();
    }
}