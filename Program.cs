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
    var cs = builder.Configuration.GetConnectionString("SisterCommunicationDb");
    options.UseSqlServer(cs);
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();