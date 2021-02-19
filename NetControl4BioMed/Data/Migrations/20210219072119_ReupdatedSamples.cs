using Microsoft.EntityFrameworkCore.Migrations;

namespace NetControl4BioMed.Data.Migrations
{
    public partial class ReupdatedSamples : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NetworkSeedData",
                table: "Samples",
                newName: "NetworkSeedNodeData");

            migrationBuilder.AddColumn<string>(
                name: "NetworkSeedEdgeData",
                table: "Samples",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetworkSeedEdgeData",
                table: "Samples");

            migrationBuilder.RenameColumn(
                name: "NetworkSeedNodeData",
                table: "Samples",
                newName: "NetworkSeedData");
        }
    }
}
