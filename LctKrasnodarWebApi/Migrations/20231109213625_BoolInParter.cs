using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LctKrasnodarWebApi.Migrations
{
    /// <inheritdoc />
    public partial class BoolInParter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PartnerInfos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PartnerInfos");
        }
    }
}
