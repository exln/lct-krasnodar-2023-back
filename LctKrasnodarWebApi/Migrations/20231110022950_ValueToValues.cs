using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LctKrasnodarWebApi.Migrations
{
    /// <inheritdoc />
    public partial class ValueToValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "ConstantTaskRules");

            migrationBuilder.AddColumn<List<string>>(
                name: "Values",
                table: "ConstantTaskRules",
                type: "text[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Values",
                table: "ConstantTaskRules");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "ConstantTaskRules",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
