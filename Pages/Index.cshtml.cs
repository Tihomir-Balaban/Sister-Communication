using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sister_Communication.Data;

namespace Sister_Communication.Pages;

public sealed class IndexModel(ILogger<IndexModel> logger, SisterCommunicationDbContext db)
    : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;
    private readonly SisterCommunicationDbContext _db = db;
    public bool CanConnectToDatabase { get; private set; }

    public void OnGet()
    {
        
    }
}