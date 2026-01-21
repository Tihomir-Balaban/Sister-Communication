using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sister_Communication.Data.Entities;
using Sister_Communication.Services.Interfaces;

namespace Sister_Communication.Pages;

public sealed class PrivacyModel(
    ILogger<PrivacyModel> logger,
    ISerpApiSearchService serpApi,
    ISearchResultStoreService store) : PageModel
{
    private readonly ILogger<PrivacyModel> _logger = logger;
    private readonly ISerpApiSearchService _serpApi = serpApi;
    private readonly ISearchResultStoreService _store = store;
    
    public string? SearchTerm { get; set; }

    public List<SearchResult> Results { get; private set; } = new();


    /// <summary>
    /// Handles GET requests for the Privacy page.
    /// </summary>
    /// <returns>A completed task representing the operation.</returns>
    public Task OnGetAsync() => Task.CompletedTask;


    /// <summary>
    /// Handles the POST request for the search action by retrieving results based on the specified search term
    /// and updating the stored search results.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete, enabling cancellation of the operation.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that renders the page either with model errors (if the input is invalid) or the updated search results.
    /// </returns>
    public async Task<IActionResult> OnPostSearchAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            ModelState.AddModelError(nameof(SearchTerm), "Please enter a search term.");
            return Page();
        }

        var query = SearchTerm.Trim();

        var items = await _serpApi.SearchAsync(query, maxResults: 100, cancellationToken);

        await _store.ReplaceResultsForQueryAsync(query, items, cancellationToken);

        Results = await _store.GetResultsForQueryAsync(query, cancellationToken);

        return Page();
    }
}