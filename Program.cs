using Microsoft.EntityFrameworkCore;
using Sister_Communication.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// EF Core + SQL Server
builder.Services.AddDbContext<SisterCommunicationDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("SisterCommunicationDb");
    options.UseSqlServer(cs);
});

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