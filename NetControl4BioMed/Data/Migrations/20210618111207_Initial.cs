using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace NetControl4BioMed.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BackgroundTasks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Databases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Databases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Interactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Networks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeToDelete = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsDemonstration = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Log = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Algorithm = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Networks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProteinCollections",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProteinCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proteins",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proteins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseInteractionFields",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatabaseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSearchable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseInteractionFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatabaseInteractionFields_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseProteinFields",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatabaseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSearchable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseProteinFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatabaseProteinFields_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseUsers",
                columns: table => new
                {
                    DatabaseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseUsers", x => new { x.DatabaseId, x.Email });
                    table.ForeignKey(
                        name: "FK_DatabaseUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DatabaseUsers_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseInteractions",
                columns: table => new
                {
                    DatabaseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InteractionId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseInteractions", x => new { x.DatabaseId, x.InteractionId });
                    table.ForeignKey(
                        name: "FK_DatabaseInteractions_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DatabaseInteractions_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Analyses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeStarted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeEnded = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeToDelete = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsDemonstration = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Log = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentIteration = table.Column<int>(type: "int", nullable: false),
                    CurrentIterationWithoutImprovement = table.Column<int>(type: "int", nullable: false),
                    MaximumIterations = table.Column<int>(type: "int", nullable: false),
                    MaximumIterationsWithoutImprovement = table.Column<int>(type: "int", nullable: false),
                    Algorithm = table.Column<int>(type: "int", nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NetworkId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Analyses_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkDatabases",
                columns: table => new
                {
                    NetworkId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DatabaseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkDatabases", x => new { x.NetworkId, x.DatabaseId, x.Type });
                    table.ForeignKey(
                        name: "FK_NetworkDatabases_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetworkDatabases_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkInteractions",
                columns: table => new
                {
                    NetworkId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InteractionId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkInteractions", x => new { x.NetworkId, x.InteractionId });
                    table.ForeignKey(
                        name: "FK_NetworkInteractions_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetworkInteractions_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkUsers",
                columns: table => new
                {
                    NetworkId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkUsers", x => new { x.NetworkId, x.Email, x.Type });
                    table.ForeignKey(
                        name: "FK_NetworkUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NetworkUsers_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkProteinCollections",
                columns: table => new
                {
                    NetworkId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProteinCollectionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkProteinCollections", x => new { x.NetworkId, x.ProteinCollectionId, x.Type });
                    table.ForeignKey(
                        name: "FK_NetworkProteinCollections_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetworkProteinCollections_ProteinCollections_ProteinCollectionId",
                        column: x => x.ProteinCollectionId,
                        principalTable: "ProteinCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProteinCollectionTypes",
                columns: table => new
                {
                    ProteinCollectionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProteinCollectionTypes", x => new { x.ProteinCollectionId, x.Type });
                    table.ForeignKey(
                        name: "FK_ProteinCollectionTypes_ProteinCollections_ProteinCollectionId",
                        column: x => x.ProteinCollectionId,
                        principalTable: "ProteinCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseProteins",
                columns: table => new
                {
                    DatabaseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProteinId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseProteins", x => new { x.DatabaseId, x.ProteinId });
                    table.ForeignKey(
                        name: "FK_DatabaseProteins_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DatabaseProteins_Proteins_ProteinId",
                        column: x => x.ProteinId,
                        principalTable: "Proteins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InteractionProteins",
                columns: table => new
                {
                    InteractionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProteinId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionProteins", x => new { x.InteractionId, x.ProteinId, x.Type });
                    table.ForeignKey(
                        name: "FK_InteractionProteins_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InteractionProteins_Proteins_ProteinId",
                        column: x => x.ProteinId,
                        principalTable: "Proteins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkProteins",
                columns: table => new
                {
                    NetworkId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProteinId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkProteins", x => new { x.NetworkId, x.ProteinId, x.Type });
                    table.ForeignKey(
                        name: "FK_NetworkProteins_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetworkProteins_Proteins_ProteinId",
                        column: x => x.ProteinId,
                        principalTable: "Proteins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProteinCollectionProteins",
                columns: table => new
                {
                    ProteinCollectionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProteinId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProteinCollectionProteins", x => new { x.ProteinCollectionId, x.ProteinId });
                    table.ForeignKey(
                        name: "FK_ProteinCollectionProteins_ProteinCollections_ProteinCollectionId",
                        column: x => x.ProteinCollectionId,
                        principalTable: "ProteinCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProteinCollectionProteins_Proteins_ProteinId",
                        column: x => x.ProteinId,
                        principalTable: "Proteins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseInteractionFieldInteractions",
                columns: table => new
                {
                    DatabaseInteractionFieldId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InteractionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseInteractionFieldInteractions", x => new { x.DatabaseInteractionFieldId, x.InteractionId, x.Value });
                    table.ForeignKey(
                        name: "FK_DatabaseInteractionFieldInteractions_DatabaseInteractionFields_DatabaseInteractionFieldId",
                        column: x => x.DatabaseInteractionFieldId,
                        principalTable: "DatabaseInteractionFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DatabaseInteractionFieldInteractions_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseProteinFieldProteins",
                columns: table => new
                {
                    DatabaseProteinFieldId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProteinId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseProteinFieldProteins", x => new { x.DatabaseProteinFieldId, x.ProteinId, x.Value });
                    table.ForeignKey(
                        name: "FK_DatabaseProteinFieldProteins_DatabaseProteinFields_DatabaseProteinFieldId",
                        column: x => x.DatabaseProteinFieldId,
                        principalTable: "DatabaseProteinFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DatabaseProteinFieldProteins_Proteins_ProteinId",
                        column: x => x.ProteinId,
                        principalTable: "Proteins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisDatabases",
                columns: table => new
                {
                    AnalysisId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DatabaseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisDatabases", x => new { x.AnalysisId, x.DatabaseId, x.Type });
                    table.ForeignKey(
                        name: "FK_AnalysisDatabases_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalysisDatabases_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisInteractions",
                columns: table => new
                {
                    AnalysisId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InteractionId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisInteractions", x => new { x.AnalysisId, x.InteractionId });
                    table.ForeignKey(
                        name: "FK_AnalysisInteractions_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalysisInteractions_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisProteinCollections",
                columns: table => new
                {
                    AnalysisId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProteinCollectionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisProteinCollections", x => new { x.AnalysisId, x.ProteinCollectionId, x.Type });
                    table.ForeignKey(
                        name: "FK_AnalysisProteinCollections_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalysisProteinCollections_ProteinCollections_ProteinCollectionId",
                        column: x => x.ProteinCollectionId,
                        principalTable: "ProteinCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisProteins",
                columns: table => new
                {
                    AnalysisId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProteinId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisProteins", x => new { x.AnalysisId, x.ProteinId, x.Type });
                    table.ForeignKey(
                        name: "FK_AnalysisProteins_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalysisProteins_Proteins_ProteinId",
                        column: x => x.ProteinId,
                        principalTable: "Proteins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisUsers",
                columns: table => new
                {
                    AnalysisId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisUsers", x => new { x.AnalysisId, x.Email, x.Type });
                    table.ForeignKey(
                        name: "FK_AnalysisUsers_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalysisUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ControlPaths",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AnalysisId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlPaths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlPaths_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paths",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ControlPathId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paths_ControlPaths_ControlPathId",
                        column: x => x.ControlPathId,
                        principalTable: "ControlPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PathInteractions",
                columns: table => new
                {
                    PathId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InteractionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathInteractions", x => new { x.PathId, x.InteractionId, x.Index });
                    table.ForeignKey(
                        name: "FK_PathInteractions_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PathInteractions_Paths_PathId",
                        column: x => x.PathId,
                        principalTable: "Paths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PathProteins",
                columns: table => new
                {
                    PathId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProteinId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathProteins", x => new { x.PathId, x.ProteinId, x.Type, x.Index });
                    table.ForeignKey(
                        name: "FK_PathProteins_Paths_PathId",
                        column: x => x.PathId,
                        principalTable: "Paths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PathProteins_Proteins_ProteinId",
                        column: x => x.ProteinId,
                        principalTable: "Proteins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Analyses_NetworkId",
                table: "Analyses",
                column: "NetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisDatabases_DatabaseId",
                table: "AnalysisDatabases",
                column: "DatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisInteractions_InteractionId",
                table: "AnalysisInteractions",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisProteinCollections_ProteinCollectionId",
                table: "AnalysisProteinCollections",
                column: "ProteinCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisProteins_ProteinId",
                table: "AnalysisProteins",
                column: "ProteinId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisUsers_UserId",
                table: "AnalysisUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ControlPaths_AnalysisId",
                table: "ControlPaths",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseInteractionFieldInteractions_InteractionId",
                table: "DatabaseInteractionFieldInteractions",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseInteractionFields_DatabaseId",
                table: "DatabaseInteractionFields",
                column: "DatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseInteractions_InteractionId",
                table: "DatabaseInteractions",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseProteinFieldProteins_ProteinId",
                table: "DatabaseProteinFieldProteins",
                column: "ProteinId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseProteinFields_DatabaseId",
                table: "DatabaseProteinFields",
                column: "DatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseProteins_ProteinId",
                table: "DatabaseProteins",
                column: "ProteinId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseUsers_UserId",
                table: "DatabaseUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionProteins_ProteinId",
                table: "InteractionProteins",
                column: "ProteinId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkDatabases_DatabaseId",
                table: "NetworkDatabases",
                column: "DatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkInteractions_InteractionId",
                table: "NetworkInteractions",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkProteinCollections_ProteinCollectionId",
                table: "NetworkProteinCollections",
                column: "ProteinCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkProteins_ProteinId",
                table: "NetworkProteins",
                column: "ProteinId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkUsers_UserId",
                table: "NetworkUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PathInteractions_InteractionId",
                table: "PathInteractions",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_PathProteins_ProteinId",
                table: "PathProteins",
                column: "ProteinId");

            migrationBuilder.CreateIndex(
                name: "IX_Paths_ControlPathId",
                table: "Paths",
                column: "ControlPathId");

            migrationBuilder.CreateIndex(
                name: "IX_ProteinCollectionProteins_ProteinId",
                table: "ProteinCollectionProteins",
                column: "ProteinId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisDatabases");

            migrationBuilder.DropTable(
                name: "AnalysisInteractions");

            migrationBuilder.DropTable(
                name: "AnalysisProteinCollections");

            migrationBuilder.DropTable(
                name: "AnalysisProteins");

            migrationBuilder.DropTable(
                name: "AnalysisUsers");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BackgroundTasks");

            migrationBuilder.DropTable(
                name: "DatabaseInteractionFieldInteractions");

            migrationBuilder.DropTable(
                name: "DatabaseInteractions");

            migrationBuilder.DropTable(
                name: "DatabaseProteinFieldProteins");

            migrationBuilder.DropTable(
                name: "DatabaseProteins");

            migrationBuilder.DropTable(
                name: "DatabaseUsers");

            migrationBuilder.DropTable(
                name: "InteractionProteins");

            migrationBuilder.DropTable(
                name: "NetworkDatabases");

            migrationBuilder.DropTable(
                name: "NetworkInteractions");

            migrationBuilder.DropTable(
                name: "NetworkProteinCollections");

            migrationBuilder.DropTable(
                name: "NetworkProteins");

            migrationBuilder.DropTable(
                name: "NetworkUsers");

            migrationBuilder.DropTable(
                name: "PathInteractions");

            migrationBuilder.DropTable(
                name: "PathProteins");

            migrationBuilder.DropTable(
                name: "ProteinCollectionProteins");

            migrationBuilder.DropTable(
                name: "ProteinCollectionTypes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "DatabaseInteractionFields");

            migrationBuilder.DropTable(
                name: "DatabaseProteinFields");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Interactions");

            migrationBuilder.DropTable(
                name: "Paths");

            migrationBuilder.DropTable(
                name: "Proteins");

            migrationBuilder.DropTable(
                name: "ProteinCollections");

            migrationBuilder.DropTable(
                name: "Databases");

            migrationBuilder.DropTable(
                name: "ControlPaths");

            migrationBuilder.DropTable(
                name: "Analyses");

            migrationBuilder.DropTable(
                name: "Networks");
        }
    }
}
