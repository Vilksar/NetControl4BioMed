using Microsoft.EntityFrameworkCore.Migrations;

namespace NetControl4BioMed.Data.Migrations
{
    public partial class FixedSamples : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AnalysisTargetData",
                table: "Samples",
                newName: "AnalysisTargetNodeData");

            migrationBuilder.RenameColumn(
                name: "AnalysisSourceData",
                table: "Samples",
                newName: "AnalysisSourceNodeData");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AnalysisTargetNodeData",
                table: "Samples",
                newName: "AnalysisTargetData");

            migrationBuilder.RenameColumn(
                name: "AnalysisSourceNodeData",
                table: "Samples",
                newName: "AnalysisSourceData");
        }
    }
}
