using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data
{
    /// <summary>
    /// Represents the database context of the application.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<User, Role, string, IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        /// <summary>
        /// Represents the default batch size for a database operation.
        /// </summary>
        public static int BatchSize { get; } = 200;

        /// <summary>
        /// Gets or sets the database table containing the analyses.
        /// </summary>
        public DbSet<Analysis> Analyses { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between analyses and edges.
        /// </summary>
        public DbSet<AnalysisEdge> AnalysisEdges { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between analyses and networks.
        /// </summary>
        public DbSet<AnalysisNetwork> AnalysisNetworks { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between analyses and nodes.
        /// </summary>
        public DbSet<AnalysisNode> AnalysisNodes { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between analyses and node collections.
        /// </summary>
        public DbSet<AnalysisNodeCollection> AnalysisNodeCollections { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between analyses and regsitered users.
        /// </summary>
        public DbSet<AnalysisUser> AnalysisUsers { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between analyses and unregistered users.
        /// </summary>
        public DbSet<AnalysisUserInvitation> AnalysisUserInvitations { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the background tasks.
        /// </summary>
        public DbSet<BackgroundTask> BackgroundTasks { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the control paths for analyses.
        /// </summary>
        public DbSet<ControlPath> ControlPaths { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the databases.
        /// </summary>
        public DbSet<Database> Databases { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between databases and edges.
        /// </summary>
        public DbSet<DatabaseEdge> DatabaseEdges { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the database edge fields.
        /// </summary>
        public DbSet<DatabaseEdgeField> DatabaseEdgeFields { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between database edge fields and edges.
        /// </summary>
        public DbSet<DatabaseEdgeFieldEdge> DatabaseEdgeFieldEdges { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between databases and nodes.
        /// </summary>
        public DbSet<DatabaseNode> DatabaseNodes { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the database node fields.
        /// </summary>
        public DbSet<DatabaseNodeField> DatabaseNodeFields { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between database node fields and nodes.
        /// </summary>
        public DbSet<DatabaseNodeFieldNode> DatabaseNodeFieldNodes { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the databases.
        /// </summary>
        public DbSet<DatabaseType> DatabaseTypes { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between databases and users.
        /// </summary>
        public DbSet<DatabaseUser> DatabaseUsers { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between databases and unregistered users.
        /// </summary>
        public DbSet<DatabaseUserInvitation> DatabaseUserInvitations { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the edges.
        /// </summary>
        public DbSet<Edge> Edges { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between edges and nodes.
        /// </summary>
        public DbSet<EdgeNode> EdgeNodes { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the networks.
        /// </summary>
        public DbSet<Network> Networks { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between networks and databases.
        /// </summary>
        public DbSet<NetworkDatabase> NetworkDatabases { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between networks and edges.
        /// </summary>
        public DbSet<NetworkEdge> NetworkEdges { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between networks and nodes.
        /// </summary>
        public DbSet<NetworkNode> NetworkNodes { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between networks and node collections.
        /// </summary>
        public DbSet<NetworkNodeCollection> NetworkNodeCollections { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between networks and users.
        /// </summary>
        public DbSet<NetworkUser> NetworkUsers { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between networks and unregistered users.
        /// </summary>
        public DbSet<NetworkUserInvitation> NetworkUserInvitations { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the nodes.
        /// </summary>
        public DbSet<Node> Nodes { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the node collections.
        /// </summary>
        public DbSet<NodeCollection> NodeCollections { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between databases and node collections.
        /// </summary>
        public DbSet<NodeCollectionDatabase> NodeCollectionDatabases { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between node collections and nodes.
        /// </summary>
        public DbSet<NodeCollectionNode> NodeCollectionNodes { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the paths in control paths for analyses.
        /// </summary>
        public DbSet<Path> Paths { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between paths and edges.
        /// </summary>
        public DbSet<PathEdge> PathEdges { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between paths and nodes.
        /// </summary>
        public DbSet<PathNode> PathNodes { get; set; }

        /// <summary>
        /// Initializes a new instance of the database context.
        /// </summary>
        /// <param name="options">Represents the options for the database context.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        /// <summary>
        /// Configures code-first the database entities and relationships between them.
        /// </summary>
        /// <param name="modelBuilder">Represents the model builder that will be in charge of building the database.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the schema needed for Identity.
            base.OnModelCreating(modelBuilder);
            // Configure the IDs, one-to-many and many-to-many relationships for the data.
            modelBuilder.Entity<Analysis>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<AnalysisDatabase>(entity =>
            {
                entity.HasKey(item => new { item.AnalysisId, item.DatabaseId });
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisDatabases)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.AnalysisDatabases)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisEdge>(entity =>
            {
                entity.HasKey(item => new { item.AnalysisId, item.EdgeId });
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisEdges)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                entity.HasOne(item => item.Edge)
                    .WithMany(item => item.AnalysisEdges)
                    .HasForeignKey(item => item.EdgeId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisNetwork>(entity =>
            {
                entity.HasKey(item => new { item.AnalysisId, item.NetworkId });
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisNetworks)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.AnalysisNetworks)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisNode>(entity =>
            {
                entity.HasKey(item => new { item.AnalysisId, item.NodeId, item.Type });
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisNodes)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                entity.HasOne(item => item.Node)
                    .WithMany(item => item.AnalysisNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisNodeCollection>(entity =>
            {
                entity.HasKey(item => new { item.AnalysisId, item.NodeCollectionId, item.Type });
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisNodeCollections)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                entity.HasOne(item => item.NodeCollection)
                    .WithMany(item => item.AnalysisNodeCollections)
                    .HasForeignKey(item => item.NodeCollectionId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisUser>(entity =>
            {
                entity.HasKey(item => new { item.AnalysisId, item.UserId });
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisUsers)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                entity.HasOne(item => item.User)
                    .WithMany(item => item.AnalysisUsers)
                    .HasForeignKey(item => item.UserId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisUserInvitation>(entity =>
            {
                entity.HasKey(item => new { item.AnalysisId, item.Email });
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisUserInvitations)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
            });
            modelBuilder.Entity<BackgroundTask>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<ControlPath>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.ControlPaths)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
            });
            modelBuilder.Entity<Database>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
                entity.HasOne(item => item.DatabaseType)
                    .WithMany(item => item.Databases)
                    .HasForeignKey(item => item.DatabaseTypeId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseEdge>(entity =>
            {
                entity.HasKey(item => new { item.DatabaseId, item.EdgeId });
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseEdges)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
                entity.HasOne(item => item.Edge)
                    .WithMany(item => item.DatabaseEdges)
                    .HasForeignKey(item => item.EdgeId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseEdgeField>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseEdgeFields)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseEdgeFieldEdge>(entity =>
            {
                entity.HasKey(item => new { item.DatabaseEdgeFieldId, item.EdgeId, item.Value });
                entity.HasOne(item => item.DatabaseEdgeField)
                    .WithMany(item => item.DatabaseEdgeFieldEdges)
                    .HasForeignKey(item => item.DatabaseEdgeFieldId)
                    .IsRequired();
                entity.HasOne(item => item.Edge)
                    .WithMany(item => item.DatabaseEdgeFieldEdges)
                    .HasForeignKey(item => item.EdgeId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseNode>(entity =>
            {
                entity.HasKey(item => new { item.DatabaseId, item.NodeId });
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseNodes)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
                entity.HasOne(item => item.Node)
                    .WithMany(item => item.DatabaseNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseNodeField>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseNodeFields)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseNodeFieldNode>(entity =>
            {
                entity.HasKey(item => new { item.DatabaseNodeFieldId, item.NodeId, item.Value });
                entity.HasOne(item => item.DatabaseNodeField)
                    .WithMany(item => item.DatabaseNodeFieldNodes)
                    .HasForeignKey(item => item.DatabaseNodeFieldId)
                    .IsRequired();
                entity.HasOne(item => item.Node)
                    .WithMany(item => item.DatabaseNodeFieldNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseType>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<DatabaseUser>(entity =>
            {
                entity.HasKey(item => new { item.DatabaseId, item.UserId });
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseUsers)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
                entity.HasOne(item => item.User)
                    .WithMany(item => item.DatabaseUsers)
                    .HasForeignKey(item => item.UserId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseUserInvitation>(entity =>
            {
                entity.HasKey(item => new { item.DatabaseId, item.Email });
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseUserInvitations)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<Edge>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<EdgeNode>(entity =>
            {
                entity.HasKey(item => new { item.EdgeId, item.NodeId, item.Type });
                entity.HasOne(item => item.Edge)
                    .WithMany(item => item.EdgeNodes)
                    .HasForeignKey(item => item.EdgeId)
                    .IsRequired();
                entity.HasOne(item => item.Node)
                    .WithMany(item => item.EdgeNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<Network>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<NetworkDatabase>(entity =>
            {
                entity.HasKey(item => new { item.NetworkId, item.DatabaseId });
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkDatabases)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.NetworkDatabases)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkEdge>(entity =>
            {
                entity.HasKey(item => new { item.NetworkId, item.EdgeId });
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkEdges)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                entity.HasOne(item => item.Edge)
                    .WithMany(item => item.NetworkEdges)
                    .HasForeignKey(item => item.EdgeId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkNode>(entity =>
            {
                entity.HasKey(item => new { item.NetworkId, item.NodeId, item.Type });
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkNodes)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                entity.HasOne(item => item.Node)
                    .WithMany(item => item.NetworkNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkNodeCollection>(entity =>
            {
                entity.HasKey(item => new { item.NetworkId, item.NodeCollectionId, item.Type });
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkNodeCollections)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                entity.HasOne(item => item.NodeCollection)
                    .WithMany(item => item.NetworkNodeCollections)
                    .HasForeignKey(item => item.NodeCollectionId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkUser>(entity =>
            {
                entity.HasKey(item => new { item.NetworkId, item.UserId });
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkUsers)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                entity.HasOne(item => item.User)
                    .WithMany(item => item.NetworkUsers)
                    .HasForeignKey(item => item.UserId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkUserInvitation>(entity =>
            {
                entity.HasKey(item => new { item.NetworkId, item.Email });
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkUserInvitations)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
            });
            modelBuilder.Entity<Node>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<NodeCollection>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<NodeCollectionDatabase>(entity =>
            {
                entity.HasKey(item => new { item.NodeCollectionId, item.DatabaseId });
                entity.HasOne(item => item.NodeCollection)
                    .WithMany(item => item.NodeCollectionDatabases)
                    .HasForeignKey(item => item.NodeCollectionId)
                    .IsRequired();
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.NodeCollectionDatabases)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<NodeCollectionNode>(entity =>
            {
                entity.HasKey(item => new { item.NodeCollectionId, item.NodeId });
                entity.HasOne(item => item.NodeCollection)
                    .WithMany(item => item.NodeCollectionNodes)
                    .HasForeignKey(item => item.NodeCollectionId)
                    .IsRequired();
                entity.HasOne(item => item.Node)
                    .WithMany(item => item.NodeCollectionNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<Path>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
                entity.HasOne(item => item.ControlPath)
                    .WithMany(item => item.Paths)
                    .HasForeignKey(item => item.ControlPathId)
                    .IsRequired();
            });
            modelBuilder.Entity<PathEdge>(entity =>
            {
                entity.HasKey(item => new { item.PathId, item.EdgeId });
                entity.HasOne(item => item.Path)
                    .WithMany(item => item.PathEdges)
                    .HasForeignKey(item => item.PathId)
                    .IsRequired();
                entity.HasOne(item => item.Edge)
                    .WithMany(item => item.PathEdges)
                    .HasForeignKey(item => item.EdgeId)
                    .IsRequired();
            });
            modelBuilder.Entity<PathNode>(entity =>
            {
                entity.HasKey(item => new { item.PathId, item.NodeId, item.Type });
                entity.HasOne(item => item.Path)
                    .WithMany(item => item.PathNodes)
                    .HasForeignKey(item => item.PathId)
                    .IsRequired();
                entity.HasOne(item => item.Node)
                    .WithMany(item => item.PathNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(item => new { item.UserId, item.RoleId });
                entity.HasOne(item => item.User)
                    .WithMany(item => item.UserRoles)
                    .HasForeignKey(item => item.UserId)
                    .IsRequired();
                entity.HasOne(item => item.Role)
                    .WithMany(item => item.UserRoles)
                    .HasForeignKey(item => item.RoleId)
                    .IsRequired();
            });
        }
    }
}
