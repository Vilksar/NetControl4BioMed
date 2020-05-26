using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NetControl4BioMed.Data.Migrations
{
    public partial class Current : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NetworkDatabases",
                table: "NetworkDatabases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnalysisDatabase",
                table: "AnalysisDatabase");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "Analyses");

            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "Networks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Log",
                table: "Networks",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Networks",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "NetworkDatabases",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "AnalysisDatabase",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "Analyses",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTimeCreated",
                table: "Analyses",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_NetworkDatabases",
                table: "NetworkDatabases",
                columns: new[] { "NetworkId", "DatabaseId", "Type" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnalysisDatabase",
                table: "AnalysisDatabase",
                columns: new[] { "AnalysisId", "DatabaseId", "Type" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NetworkDatabases",
                table: "NetworkDatabases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnalysisDatabase",
                table: "AnalysisDatabase");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "Networks");

            migrationBuilder.DropColumn(
                name: "Log",
                table: "Networks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Networks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "NetworkDatabases");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "AnalysisDatabase");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "Analyses");

            migrationBuilder.DropColumn(
                name: "DateTimeCreated",
                table: "Analyses");

            migrationBuilder.AddColumn<string>(
                name: "JobId",
                table: "Analyses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_NetworkDatabases",
                table: "NetworkDatabases",
                columns: new[] { "NetworkId", "DatabaseId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnalysisDatabase",
                table: "AnalysisDatabase",
                columns: new[] { "AnalysisId", "DatabaseId" });
        }
    }
}
