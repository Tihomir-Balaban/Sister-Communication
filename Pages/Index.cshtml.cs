using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sister_Communication.Data;

namespace Sister_Communication.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly SisterCommunicationDbContext _db;
    public bool CanConnectToDatabase { get; private set; }
    
    public IndexModel(ILogger<IndexModel> logger, SisterCommunicationDbContext db)
    {
        _logger = logger;
        _db = db;
        
    }

    // public void OnGet()
    // {
    //     
    // }
    
    public async Task OnGetAsync()
    {
        CanConnectToDatabase = await _db.Database.CanConnectAsync();
    }
}