using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LctKrasnodarWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartnerInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "text", nullable: false),
                    LocationCoordinates = table.Column<List<double>>(type: "double precision[]", nullable: false),
                    WhenPointConnected = table.Column<int>(type: "integer", nullable: true),
                    AreCardsAndMaterialsDelivered = table.Column<int>(type: "integer", nullable: true),
                    DaysSinceLastCardIssue = table.Column<int>(type: "integer", nullable: true),
                    NumberOfApprovedApplications = table.Column<int>(type: "integer", nullable: true),
                    NumberOfGivenCards = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerInfos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnerInfos");
        }
    }
}
