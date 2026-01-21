using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sister_Communication.Data.Entities;
using Sister_Communication.Services.Interfaces;

namespace Sister_Communication.Pages;

public sealed class PrivacyModel(
    ILogger<PrivacyModel> logger,
    IGoogleSearchService google,
    ISearchResultStoreService store) : PageModel
{
    private readonly ILogger<PrivacyModel> _logger = logger;
    private readonly IGoogleSearchService _google = google;
    private readonly ISearchResultStoreService _store = store;
    
    public string? SearchTerm { get; set; }

    public List<SearchResult> Results { get; private set; } = new();

    /// Handles the GET request for the Privacy page.
    /// This method is invoked when the user navigates to the Privacy page
    /// without submitting any form data or initiating a specific action.
    /// It performs any necessary initialization for the page and prepares
    /// the related view or data, if applicable.
    /// <returns>A completed Task representing the execution of the method.</returns>
    public Task OnGetAsync() => Task.CompletedTask;

    /// <summary>
    /// Handles the search functionality. This method processes the user-provided search term,
    /// performs a search via the Google search service, stores the results, and retrieves the
    /// results to display on the page.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while performing asynchronous operations.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the result of the operation.
    /// Returns the current page if successful or if validation errors occur.
    /// </returns>
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

        Results = await _store.GetResultsForQueryAsync(query, cancellationToken);

        return Page();
    }
}