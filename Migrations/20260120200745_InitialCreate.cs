using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sister_Communication.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Query = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Url = table.Column<string>(type: "varchar(2048)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Snippet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayLink = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Position = table.Column<int>(type: "int", nullable: false),
                    FetchedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchResults_Query",
                table: "SearchResults",
                column: "Query");

            migrationBuilder.CreateIndex(
                name: "IX_SearchResults_Url",
                table: "SearchResults",
                column: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchResults");
        }
    }
}
