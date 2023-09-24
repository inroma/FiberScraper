using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FiberEvolutionScraper.Api.Migrations
{
    /// <inheritdoc />
    public partial class AutoRefresh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoRefreshInput",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CoordX = table.Column<double>(type: "double precision", nullable: false),
                    CoordY = table.Column<double>(type: "double precision", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: true),
                    AreaSize = table.Column<int>(type: "integer", nullable: false),
                    LastRun = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoRefreshInput", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoRefreshInput");
        }
    }
}
