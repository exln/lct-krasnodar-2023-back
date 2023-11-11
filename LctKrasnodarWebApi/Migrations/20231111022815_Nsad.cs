using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LctKrasnodarWebApi.Migrations
{
    /// <inheritdoc />
    public partial class Nsad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "AssignedTasks",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "Tag",
                table: "AssignedTasks",
                newName: "PartnerId");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "AssignedTasks",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<int>(
                name: "TravelTime",
                table: "AssignedTasks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Polyline",
                table: "AssignedTasks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<List<double>>(
                name: "LocationCoordinatesFrom",
                table: "AssignedTasks",
                type: "double precision[]",
                nullable: true,
                oldClrType: typeof(List<double>),
                oldType: "double precision[]");

            migrationBuilder.AlterColumn<string>(
                name: "AddressFrom",
                table: "AssignedTasks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "CourierId",
                table: "AssignedTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "Grades",
                table: "AssignedTasks",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.CreateTable(
                name: "CourierDtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Grade = table.Column<int>(type: "integer", nullable: false),
                    LocationCoordinates = table.Column<List<double>>(type: "double precision[]", nullable: false),
                    WorkTime = table.Column<int>(type: "integer", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourierDtos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskDtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PartnerId = table.Column<int>(type: "integer", nullable: true),
                    LocationCoordinates = table.Column<List<double>>(type: "double precision[]", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isDone = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Grades = table.Column<int[]>(type: "integer[]", nullable: false),
                    TravelTime = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDtos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourierDtos");

            migrationBuilder.DropTable(
                name: "TaskDtos");

            migrationBuilder.DropColumn(
                name: "CourierId",
                table: "AssignedTasks");

            migrationBuilder.DropColumn(
                name: "Grades",
                table: "AssignedTasks");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "AssignedTasks",
                newName: "WorkerId");

            migrationBuilder.RenameColumn(
                name: "PartnerId",
                table: "AssignedTasks",
                newName: "Tag");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AssignedTasks",
                newName: "CreationDate");

            migrationBuilder.AlterColumn<int>(
                name: "TravelTime",
                table: "AssignedTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Polyline",
                table: "AssignedTasks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<double>>(
                name: "LocationCoordinatesFrom",
                table: "AssignedTasks",
                type: "double precision[]",
                nullable: false,
                oldClrType: typeof(List<double>),
                oldType: "double precision[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressFrom",
                table: "AssignedTasks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
