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

    /// <summary>
    /// Handles the GET request for the page and performs any necessary initialization or setup.
    /// This method is called when the page is loaded via an HTTP GET request.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task OnGetAsync() => Task.CompletedTask;

    /// <summary>
    /// Handles the search postback by validating the search term, fetching results using the search API,
    /// and updating the search result store with the latest results.
    /// </summary>
    /// <param name="cancellationToken">Token for canceling the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that renders the current page with the updated search results.
    /// Returns the current page with a validation error if the search term is not provided.
    /// </returns>
    public async Task<IActionResult> OnPostSearchAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            ModelState.AddModelError(nameof(SearchTerm), "Please enter a search term.");
            return Page();
        }

        var query = SearchTerm.Trim();
        
        var items = await serpApi.SearchAsync(query, maxResults: 100, cancellationToken);

        await _store.ReplaceResultsForQueryAsync(query, items, cancellationToken);

        CurrentQuery = query;
        Results = await _store.GetResultsForQueryAsync(query, cancellationToken);

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

        Results = await _store.FilterResultsAsync(CurrentQuery, DbFilterTerm, cancellationToken);

        return Page();
    }
}