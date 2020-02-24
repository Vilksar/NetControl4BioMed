using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NetControl4BioMed.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Analyses",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DateTimeStarted = table.Column<DateTime>(nullable: true),
                    DateTimeEnded = table.Column<DateTime>(nullable: true),
                    DateTimeIntervals = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Log = table.Column<string>(nullable: true),
                    CurrentIteration = table.Column<int>(nullable: false),
                    CurrentIterationWithoutImprovement = table.Column<int>(nullable: false),
                    Algorithm = table.Column<int>(nullable: false),
                    Parameters = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analyses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    DateTimeCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseTypes",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Edges",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Networks",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Algorithm = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Networks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NodeCollections",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisUserInvitations",
                columns: table => new
                {
                    AnalysisId = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisUserInvitations", x => new { x.AnalysisId, x.Email });
                    table.ForeignKey(
                        name: "FK_AnalysisUserInvitations_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ControlPaths",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AnalysisId = table.Column<string>(nullable: false)
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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
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
                name: "AnalysisUsers",
                columns: table => new
                {
                    AnalysisId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisUsers", x => new { x.AnalysisId, x.UserId });
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
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
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
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
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
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
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
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
                name: "Databases",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    IsPublic = table.Column<bool>(nullable: false),
                    DatabaseTypeId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Databases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Databases_DatabaseTypes_DatabaseTypeId",
                        column: x => x.DatabaseTypeId,
                        principalTable: "DatabaseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisEdges",
                columns: table => new
                {
                    AnalysisId = table.Column<string>(nullable: false),
                    EdgeId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisEdges", x => new { x.AnalysisId, x.EdgeId });
                    table.ForeignKey(
                        name: "FK_AnalysisEdges_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalysisEdges_Edges_EdgeId",
                        column: x => x.EdgeId,
                        principalTable: "Edges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisNetworks",
                columns: table => new
                {
                    AnalysisId = table.Column<string>(nullable: false),
                    NetworkId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisNetworks", x => new { x.AnalysisId, x.NetworkId });
                    table.ForeignKey(
                        name: "FK_AnalysisNetworks_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalysisNetworks_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkEdges",
                columns: table => new
                {
                    NetworkId = table.Column<string>(nullable: false),
                    EdgeId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkEdges", x => new { x.NetworkId, x.EdgeId });
                    table.ForeignKey(
                        name: "FK_NetworkEdges_Edges_EdgeId",
                        column: x => x.EdgeId,
                        principalTable: "Edges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetworkEdges_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkUserInvitations",
                columns: table => new
                {
                    NetworkId = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkUserInvitations", x => new { x.NetworkId, x.Email });
                    table.ForeignKey(
                        name: "FK_NetworkUserInvitations_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkUsers",
                columns: table => new
                {
                    NetworkId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkUsers", x => new { x.NetworkId, x.UserId });
                    table.ForeignKey(
                        name: "FK_NetworkUsers_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetworkUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisNodeCollections",
                columns: table => new
                {
                    AnalysisId = table.Column<string>(nullable: false),
                    NodeCollectionId = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisNodeCollections", x => new { x.AnalysisId, x.NodeCollectionId, x.Type });
                    table.ForeignKey(
                        name: "FK_AnalysisNodeCollections_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalysisNodeCollections_NodeCollections_NodeCollectionId",
                        column: x => x.NodeCollectionId,
                        principalTable: "NodeCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkNodeCollections",
                columns: table => new
                {
                    NetworkId = table.Column<string>(nullable: false),
                    NodeCollectionId = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkNodeCollections", x => new { x.NetworkId, x.NodeCollectionId, x.Type });
                    table.ForeignKey(
                        name: "FK_NetworkNodeCollections_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetworkNodeCollections_NodeCollections_NodeCollectionId",
                        column: x => x.NodeCollectionId,
                        principalTable: "NodeCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisNodes",
                columns: table => new
                {
                    AnalysisId = table.Column<string>(nullable: false),
                    NodeId = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisNodes", x => new { x.AnalysisId, x.NodeId, x.Type });
                    table.ForeignKey(
                        name: "FK_AnalysisNodes_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalysisNodes_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EdgeNodes",
                columns: table => new
                {
                    EdgeId = table.Column<string>(nullable: false),
                    NodeId = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdgeNodes", x => new { x.EdgeId, x.NodeId, x.Type });
                    table.ForeignKey(
                        name: "FK_EdgeNodes_Edges_EdgeId",
                        column: x => x.EdgeId,
                        principalTable: "Edges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EdgeNodes_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkNodes",
                columns: table => new
                {
                    NetworkId = table.Column<string>(nullable: false),
                    NodeId = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkNodes", x => new { x.NetworkId, x.NodeId, x.Type });
                    table.ForeignKey(
                        name: "FK_NetworkNodes_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetworkNodes_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodeCollectionNodes",
                columns: table => new
                {
                    NodeCollectionId = table.Column<string>(nullable: false),
                    NodeId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeCollectionNodes", x => new { x.NodeCollectionId, x.NodeId });
                    table.ForeignKey(
                        name: "FK_NodeCollectionNodes_NodeCollections_NodeCollectionId",
                        column: x => x.NodeCollectionId,
                        principalTable: "NodeCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NodeCollectionNodes_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paths",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ControlPathId = table.Column<string>(nullable: false)
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
                name: "AnalysisDatabase",
                columns: table => new
                {
                    AnalysisId = table.Column<string>(nullable: false),
                    DatabaseId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisDatabase", x => new { x.AnalysisId, x.DatabaseId });
                    table.ForeignKey(
                        name: "FK_AnalysisDatabase_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalysisDatabase_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseEdgeFields",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false),
                    DatabaseId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseEdgeFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatabaseEdgeFields_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseEdges",
                columns: table => new
                {
                    DatabaseId = table.Column<string>(nullable: false),
                    EdgeId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseEdges", x => new { x.DatabaseId, x.EdgeId });
                    table.ForeignKey(
                        name: "FK_DatabaseEdges_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DatabaseEdges_Edges_EdgeId",
                        column: x => x.EdgeId,
                        principalTable: "Edges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseNodeFields",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false),
                    DatabaseId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    IsSearchable = table.Column<bool>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseNodeFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatabaseNodeFields_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseNodes",
                columns: table => new
                {
                    DatabaseId = table.Column<string>(nullable: false),
                    NodeId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseNodes", x => new { x.DatabaseId, x.NodeId });
                    table.ForeignKey(
                        name: "FK_DatabaseNodes_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DatabaseNodes_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseUserInvitations",
                columns: table => new
                {
                    DatabaseId = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseUserInvitations", x => new { x.DatabaseId, x.Email });
                    table.ForeignKey(
                        name: "FK_DatabaseUserInvitations_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseUsers",
                columns: table => new
                {
                    DatabaseId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    DateTimeCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseUsers", x => new { x.DatabaseId, x.UserId });
                    table.ForeignKey(
                        name: "FK_DatabaseUsers_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DatabaseUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkDatabases",
                columns: table => new
                {
                    NetworkId = table.Column<string>(nullable: false),
                    DatabaseId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkDatabases", x => new { x.NetworkId, x.DatabaseId });
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
                name: "NodeCollectionDatabases",
                columns: table => new
                {
                    NodeCollectionId = table.Column<string>(nullable: false),
                    DatabaseId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeCollectionDatabases", x => new { x.NodeCollectionId, x.DatabaseId });
                    table.ForeignKey(
                        name: "FK_NodeCollectionDatabases_Databases_DatabaseId",
                        column: x => x.DatabaseId,
                        principalTable: "Databases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NodeCollectionDatabases_NodeCollections_NodeCollectionId",
                        column: x => x.NodeCollectionId,
                        principalTable: "NodeCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PathEdges",
                columns: table => new
                {
                    PathId = table.Column<string>(nullable: false),
                    EdgeId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathEdges", x => new { x.PathId, x.EdgeId });
                    table.ForeignKey(
                        name: "FK_PathEdges_Edges_EdgeId",
                        column: x => x.EdgeId,
                        principalTable: "Edges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PathEdges_Paths_PathId",
                        column: x => x.PathId,
                        principalTable: "Paths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PathNodes",
                columns: table => new
                {
                    PathId = table.Column<string>(nullable: false),
                    NodeId = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathNodes", x => new { x.PathId, x.NodeId, x.Type });
                    table.ForeignKey(
                        name: "FK_PathNodes_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PathNodes_Paths_PathId",
                        column: x => x.PathId,
                        principalTable: "Paths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseEdgeFieldEdges",
                columns: table => new
                {
                    DatabaseEdgeFieldId = table.Column<string>(nullable: false),
                    EdgeId = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseEdgeFieldEdges", x => new { x.DatabaseEdgeFieldId, x.EdgeId, x.Value });
                    table.ForeignKey(
                        name: "FK_DatabaseEdgeFieldEdges_DatabaseEdgeFields_DatabaseEdgeFieldId",
                        column: x => x.DatabaseEdgeFieldId,
                        principalTable: "DatabaseEdgeFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DatabaseEdgeFieldEdges_Edges_EdgeId",
                        column: x => x.EdgeId,
                        principalTable: "Edges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseNodeFieldNodes",
                columns: table => new
                {
                    DatabaseNodeFieldId = table.Column<string>(nullable: false),
                    NodeId = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseNodeFieldNodes", x => new { x.DatabaseNodeFieldId, x.NodeId, x.Value });
                    table.ForeignKey(
                        name: "FK_DatabaseNodeFieldNodes_DatabaseNodeFields_DatabaseNodeFieldId",
                        column: x => x.DatabaseNodeFieldId,
                        principalTable: "DatabaseNodeFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DatabaseNodeFieldNodes_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisDatabase_DatabaseId",
                table: "AnalysisDatabase",
                column: "DatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisEdges_EdgeId",
                table: "AnalysisEdges",
                column: "EdgeId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisNetworks_NetworkId",
                table: "AnalysisNetworks",
                column: "NetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisNodeCollections_NodeCollectionId",
                table: "AnalysisNodeCollections",
                column: "NodeCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisNodes_NodeId",
                table: "AnalysisNodes",
                column: "NodeId");

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
                name: "IX_DatabaseEdgeFieldEdges_EdgeId",
                table: "DatabaseEdgeFieldEdges",
                column: "EdgeId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseEdgeFields_DatabaseId",
                table: "DatabaseEdgeFields",
                column: "DatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseEdges_EdgeId",
                table: "DatabaseEdges",
                column: "EdgeId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseNodeFieldNodes_NodeId",
                table: "DatabaseNodeFieldNodes",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseNodeFields_DatabaseId",
                table: "DatabaseNodeFields",
                column: "DatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseNodes_NodeId",
                table: "DatabaseNodes",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Databases_DatabaseTypeId",
                table: "Databases",
                column: "DatabaseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseUsers_UserId",
                table: "DatabaseUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EdgeNodes_NodeId",
                table: "EdgeNodes",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkDatabases_DatabaseId",
                table: "NetworkDatabases",
                column: "DatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkEdges_EdgeId",
                table: "NetworkEdges",
                column: "EdgeId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkNodeCollections_NodeCollectionId",
                table: "NetworkNodeCollections",
                column: "NodeCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkNodes_NodeId",
                table: "NetworkNodes",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkUsers_UserId",
                table: "NetworkUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeCollectionDatabases_DatabaseId",
                table: "NodeCollectionDatabases",
                column: "DatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeCollectionNodes_NodeId",
                table: "NodeCollectionNodes",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PathEdges_EdgeId",
                table: "PathEdges",
                column: "EdgeId");

            migrationBuilder.CreateIndex(
                name: "IX_PathNodes_NodeId",
                table: "PathNodes",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Paths_ControlPathId",
                table: "Paths",
                column: "ControlPathId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisDatabase");

            migrationBuilder.DropTable(
                name: "AnalysisEdges");

            migrationBuilder.DropTable(
                name: "AnalysisNetworks");

            migrationBuilder.DropTable(
                name: "AnalysisNodeCollections");

            migrationBuilder.DropTable(
                name: "AnalysisNodes");

            migrationBuilder.DropTable(
                name: "AnalysisUserInvitations");

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
                name: "DatabaseEdgeFieldEdges");

            migrationBuilder.DropTable(
                name: "DatabaseEdges");

            migrationBuilder.DropTable(
                name: "DatabaseNodeFieldNodes");

            migrationBuilder.DropTable(
                name: "DatabaseNodes");

            migrationBuilder.DropTable(
                name: "DatabaseUserInvitations");

            migrationBuilder.DropTable(
                name: "DatabaseUsers");

            migrationBuilder.DropTable(
                name: "EdgeNodes");

            migrationBuilder.DropTable(
                name: "NetworkDatabases");

            migrationBuilder.DropTable(
                name: "NetworkEdges");

            migrationBuilder.DropTable(
                name: "NetworkNodeCollections");

            migrationBuilder.DropTable(
                name: "NetworkNodes");

            migrationBuilder.DropTable(
                name: "NetworkUserInvitations");

            migrationBuilder.DropTable(
                name: "NetworkUsers");

            migrationBuilder.DropTable(
                name: "NodeCollectionDatabases");

            migrationBuilder.DropTable(
                name: "NodeCollectionNodes");

            migrationBuilder.DropTable(
                name: "PathEdges");

            migrationBuilder.DropTable(
                name: "PathNodes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "DatabaseEdgeFields");

            migrationBuilder.DropTable(
                name: "DatabaseNodeFields");

            migrationBuilder.DropTable(
                name: "Networks");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "NodeCollections");

            migrationBuilder.DropTable(
                name: "Edges");

            migrationBuilder.DropTable(
                name: "Nodes");

            migrationBuilder.DropTable(
                name: "Paths");

            migrationBuilder.DropTable(
                name: "Databases");

            migrationBuilder.DropTable(
                name: "ControlPaths");

            migrationBuilder.DropTable(
                name: "DatabaseTypes");

            migrationBuilder.DropTable(
                name: "Analyses");
        }
    }
}
