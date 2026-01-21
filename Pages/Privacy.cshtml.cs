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

    public Task OnGetAsync() => Task.CompletedTask;
    
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