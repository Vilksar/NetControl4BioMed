using Microsoft.EntityFrameworkCore.Migrations;

namespace NetControl4BioMed.Data.Migrations
{
    public partial class UpdatedSamplesAndNodeCollections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Samples");

            migrationBuilder.CreateTable(
                name: "NodeCollectionTypes",
                columns: table => new
                {
                    NodeCollectionId = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeCollectionTypes", x => new { x.NodeCollectionId, x.Type });
                    table.ForeignKey(
                        name: "FK_NodeCollectionTypes_NodeCollections_NodeCollectionId",
                        column: x => x.NodeCollectionId,
                        principalTable: "NodeCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SampleDatabases",
                columns: table => new
                {
                    SampleId = table.Column<string>(nullable: false),
                    DatabaseId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SampleDatabases", x => new { x.SampleId, x.DatabaseId });
                    table.ForeignKey(
                        name: "FK_SampleDatabases_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SampleDatabases_Samples_SampleId",
                        column: x => x.SampleId,
                        principalTable: "Samples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SampleTypes",
                columns: table => new
                {
                    SampleId = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_SampleDatabases_DatabaseId",
                table: "SampleDatabases",
                column: "DatabaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NodeCollectionTypes");

            migrationBuilder.DropTable(
                name: "SampleDatabases");

            migrationBuilder.DropTable(
                name: "SampleTypes");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Samples",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
