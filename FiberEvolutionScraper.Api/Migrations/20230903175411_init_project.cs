using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiberEvolutionScraper.Api.Migrations
{
    /// <inheritdoc />
    public partial class init_project : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FiberPoint",
                columns: table => new
                {
                    Signature = table.Column<string>(type: "text", nullable: false),
                    LibAdresse = table.Column<string>(type: "text", nullable: true),
                    X = table.Column<double>(type: "double precision", nullable: false),
                    Y = table.Column<double>(type: "double precision", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiberPoint", x => x.Signature);
                });

            migrationBuilder.CreateTable(
                name: "EligibiliteFtth",
                columns: table => new
                {
                    CodeImb = table.Column<string>(type: "text", nullable: false),
                    EtapeFtth = table.Column<int>(type: "integer", nullable: false),
                    FiberPointDTOSignature = table.Column<string>(type: "text", nullable: false),
                    Batiment = table.Column<string>(type: "text", nullable: true),
                    DateDebutEligibilite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibiliteFtth", x => new { x.CodeImb, x.EtapeFtth, x.FiberPointDTOSignature });
                    table.ForeignKey(
                        name: "FK_EligibiliteFtth_FiberPoint_FiberPointDTOSignature",
                        column: x => x.FiberPointDTOSignature,
                        principalTable: "FiberPoint",
                        principalColumn: "Signature",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EligibiliteFtth_FiberPointDTOSignature",
                table: "EligibiliteFtth",
                column: "FiberPointDTOSignature");

            migrationBuilder.CreateIndex(
                name: "IX_FiberPoint_Signature",
                table: "FiberPoint",
                column: "Signature",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EligibiliteFtth");

            migrationBuilder.DropTable(
                name: "FiberPoint");
        }
    }
}
