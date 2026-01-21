using Microsoft.EntityFrameworkCore;
using Sister_Communication.Data;
using Sister_Communication.Services;
using Sister_Communication.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<Sister_Communication.Options.GoogleOptions>(builder.Configuration.GetSection("Google"));

// EF Core + SQL Server
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
    
builder.Services.AddScoped<IGoogleSearchService, GoogleSearchService>();

var app = builder.Build();

// Configure the HTTP request pipeline. 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();