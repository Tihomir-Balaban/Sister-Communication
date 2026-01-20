using Microsoft.EntityFrameworkCore;
using Sister_Communication.Data.Entities;
using Sister_Communication.Data.Configurations;
             
namespace Sister_Communication.Data;
public class SisterCommunicationDbContext(DbContextOptions<SisterCommunicationDbContext> options) : DbContext(options)
{
    public DbSet<SearchResult> SearchResults => Set<SearchResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SearchResultConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}