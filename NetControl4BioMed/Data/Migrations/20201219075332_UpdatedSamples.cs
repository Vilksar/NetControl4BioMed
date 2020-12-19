using Microsoft.EntityFrameworkCore.Migrations;

namespace NetControl4BioMed.Data.Migrations
{
    public partial class UpdatedSamples : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SampleTypes");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "Samples");

            migrationBuilder.AddColumn<int>(
                name: "AnalysisAlgorithm",
                table: "Samples",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisDescription",
                table: "Samples",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisName",
                table: "Samples",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisNetworkData",
                table: "Samples",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisSourceData",
                table: "Samples",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisSourceNodeCollectionData",
                table: "Samples",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisTargetData",
                table: "Samples",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisTargetNodeCollectionData",
                table: "Samples",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NetworkAlgorithm",
                table: "Samples",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NetworkDescription",
                table: "Samples",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetworkEdgeDatabaseData",
                table: "Samples",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetworkName",
                table: "Samples",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetworkNodeDatabaseData",
                table: "Samples",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetworkSeedData",
                table: "Samples",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetworkSeedNodeCollectionData",
                table: "Samples",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalysisAlgorithm",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "AnalysisDescription",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "AnalysisName",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "AnalysisNetworkData",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "AnalysisSourceData",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "AnalysisSourceNodeCollectionData",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "AnalysisTargetData",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "AnalysisTargetNodeCollectionData",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "NetworkAlgorithm",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "NetworkDescription",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "NetworkEdgeDatabaseData",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "NetworkName",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "NetworkNodeDatabaseData",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "NetworkSeedData",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "NetworkSeedNodeCollectionData",
                table: "Samples");

            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "Samples",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SampleTypes",
                columns: table => new
                {
                    SampleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SampleTypes", x => new { x.SampleId, x.Type });
                    table.ForeignKey(
                        name: "FK_SampleTypes_Samples_SampleId",
                        column: x => x.SampleId,
                        principalTable: "Samples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
