using Microsoft.EntityFrameworkCore;
using Sister_Communication.Data;
using Sister_Communication.Services;
using Sister_Communication.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddRazorPages();
builder.Services.Configure<Sister_Communication.Options.GoogleOptions>(builder.Configuration.GetSection("Google"));

builder.Services.AddDbContext<SisterCommunicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("SisterCommunicationDb");
    options.UseSqlServer(connectionString);
});

builder.Services.AddHttpClient<GoogleSearchService>(client =>
{   
    client.BaseAddress = new Uri("https://www.googleapis.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// DI
builder.Services.AddScoped<IGoogleSearchService, GoogleSearchService>();
builder.Services.AddScoped<ISearchResultStoreService, SearchResultStoreService>();

var app = builder.Build();

// Plumbing 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Routing
app.UseRouting();

// Authorization
app.UseAuthorization();

// Endpoints
app.MapRazorPages();

app.Run();