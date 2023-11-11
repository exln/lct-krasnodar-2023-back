using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LctKrasnodarWebApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConsts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "ConstantTaskSizes");

            migrationBuilder.AddColumn<int[]>(
                name: "Grades",
                table: "ConstantTaskSizes",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grades",
                table: "ConstantTaskSizes");

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "ConstantTaskSizes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
