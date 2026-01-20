using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sister_Communication.Data.Entities;

namespace Sister_Communication.Data.Configurations;

public class SearchResultConfiguration : IEntityTypeConfiguration<SearchResult>
{
    public void Configure(EntityTypeBuilder<SearchResult> builder)
    {
        builder.ToTable("SearchResults");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Query)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasColumnType("varchar(2048)");

        builder.Property(x => x.Title)
            .HasMaxLength(512);

        builder.Property(x => x.DisplayLink)
            .HasMaxLength(256);

        builder.Property(x => x.Position)
            .IsRequired();

        builder.Property(x => x.FetchedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.Query);
        builder.HasIndex(x => x.Url);
    }
}