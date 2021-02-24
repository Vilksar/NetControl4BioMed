using Microsoft.EntityFrameworkCore.Migrations;

namespace NetControl4BioMed.Data.Migrations
{
    public partial class UpdatedSamplesAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalysisAlgorithm",
                table: "Samples");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnalysisAlgorithm",
                table: "Samples",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
