using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LctKrasnodarWebApi.Migrations
{
    /// <inheritdoc />
    public partial class NewRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Condition",
                table: "ConstantTaskRules");

            migrationBuilder.DropColumn(
                name: "Target",
                table: "ConstantTaskRules");

            migrationBuilder.AddColumn<int[]>(
                name: "Conditions",
                table: "ConstantTaskRules",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<int[]>(
                name: "Targets",
                table: "ConstantTaskRules",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Conditions",
                table: "ConstantTaskRules");

            migrationBuilder.DropColumn(
                name: "Targets",
                table: "ConstantTaskRules");

            migrationBuilder.AddColumn<int>(
                name: "Condition",
                table: "ConstantTaskRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Target",
                table: "ConstantTaskRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
