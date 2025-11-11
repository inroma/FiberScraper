using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiberEvolutionScraper.Api.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelInheritance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "AutoRefreshInput",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "AutoRefreshInput",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "AutoRefreshInput");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "AutoRefreshInput");
        }
    }
}
