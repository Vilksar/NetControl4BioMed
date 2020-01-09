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
        /// Gets or sets the database table containing the one-to-one relationship between analyses and users.
        /// </summary>
        public DbSet<AnalysisUser> AnalysisUsers { get; set; }

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
        /// Gets or sets the database table containing the nodes.
        /// </summary>
        public DbSet<Node> Nodes { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the node collections.
        /// </summary>
        public DbSet<NodeCollection> NodeCollections { get; set; }

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
            // Configure the one-to-many and many-to-many relationships for Identity.
            modelBuilder.Entity<UserRole>(userRole =>
            {
                userRole.HasKey(item => new { item.UserId, item.RoleId });
                userRole.HasOne(item => item.User)
                    .WithMany(item => item.UserRoles)
                    .HasForeignKey(item => item.UserId)
                    .IsRequired();
                userRole.HasOne(item => item.Role)
                    .WithMany(item => item.UserRoles)
                    .HasForeignKey(item => item.RoleId)
                    .IsRequired();
            });
            // Configure the one-to-many and many-to-many relationships for the data.
            modelBuilder.Entity<AnalysisEdge>(analysisEdge =>
            {
                analysisEdge.HasKey(item => new { item.AnalysisId, item.EdgeId });
                analysisEdge.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisEdges)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                analysisEdge.HasOne(item => item.Edge)
                    .WithMany(item => item.AnalysisEdges)
                    .HasForeignKey(item => item.EdgeId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisNetwork>(analysisNetwork =>
            {
                analysisNetwork.HasKey(item => new { item.AnalysisId, item.NetworkId });
                analysisNetwork.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisNetworks)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                analysisNetwork.HasOne(item => item.Network)
                    .WithMany(item => item.AnalysisNetworks)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisNode>(analysisNode =>
            {
                analysisNode.HasKey(item => new { item.AnalysisId, item.NodeId, item.Type });
                analysisNode.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisNodes)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                analysisNode.HasOne(item => item.Node)
                    .WithMany(item => item.AnalysisNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisNodeCollection>(analysisNodeCollection =>
            {
                analysisNodeCollection.HasKey(item => new { item.AnalysisId, item.NodeCollectionId, item.Type });
                analysisNodeCollection.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisNodeCollections)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                analysisNodeCollection.HasOne(item => item.NodeCollection)
                    .WithMany(item => item.AnalysisNodeCollections)
                    .HasForeignKey(item => item.NodeCollectionId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisUser>(analysisUser =>
            {
                analysisUser.HasKey(item => new { item.AnalysisId, item.Email });
                analysisUser.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisUsers)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                analysisUser.HasOne(item => item.User)
                    .WithMany(item => item.AnalysisUsers)
                    .HasForeignKey(item => item.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ControlPath>(controlPath =>
            {
                controlPath.HasOne(item => item.Analysis)
                    .WithMany(item => item.ControlPaths)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
            });
            modelBuilder.Entity<Database>(database =>
            {
                database.HasOne(item => item.DatabaseType)
                    .WithMany(item => item.Databases)
                    .HasForeignKey(item => item.DatabaseTypeId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseEdge>(databaseEdge =>
            {
                databaseEdge.HasKey(item => new { item.DatabaseId, item.EdgeId });
                databaseEdge.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseEdges)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
                databaseEdge.HasOne(item => item.Edge)
                    .WithMany(item => item.DatabaseEdges)
                    .HasForeignKey(item => item.EdgeId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseEdgeField>(databaseEdgeField =>
            {
                databaseEdgeField.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseEdgeFields)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseEdgeFieldEdge>(databaseEdgeFieldEdge =>
            {
                databaseEdgeFieldEdge.HasKey(item => new { item.DatabaseEdgeFieldId, item.EdgeId, item.Value });
                databaseEdgeFieldEdge.HasOne(item => item.DatabaseEdgeField)
                    .WithMany(item => item.DatabaseEdgeFieldEdges)
                    .HasForeignKey(item => item.DatabaseEdgeFieldId)
                    .IsRequired();
                databaseEdgeFieldEdge.HasOne(item => item.Edge)
                    .WithMany(item => item.DatabaseEdgeFieldEdges)
                    .HasForeignKey(item => item.EdgeId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseNode>(databaseNode =>
            {
                databaseNode.HasKey(item => new { item.DatabaseId, item.NodeId });
                databaseNode.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseNodes)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
                databaseNode.HasOne(item => item.Node)
                    .WithMany(item => item.DatabaseNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseNodeField>(databaseNodeField =>
            {
                databaseNodeField.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseNodeFields)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseNodeFieldNode>(databaseNodeFieldNode =>
            {
                databaseNodeFieldNode.HasKey(item => new { item.DatabaseNodeFieldId, item.NodeId, item.Value });
                databaseNodeFieldNode.HasOne(item => item.DatabaseNodeField)
                    .WithMany(item => item.DatabaseNodeFieldNodes)
                    .HasForeignKey(item => item.DatabaseNodeFieldId)
                    .IsRequired();
                databaseNodeFieldNode.HasOne(item => item.Node)
                    .WithMany(item => item.DatabaseNodeFieldNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseUser>(databaseUser =>
            {
                databaseUser.HasKey(item => new { item.DatabaseId, item.Email });
                databaseUser.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseUsers)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
                databaseUser.HasOne(item => item.User)
                    .WithMany(item => item.DatabaseUsers)
                    .HasForeignKey(item => item.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<EdgeNode>(edgeNode =>
            {
                edgeNode.HasKey(item => new { item.EdgeId, item.NodeId, item.Type });
                edgeNode.HasOne(item => item.Edge)
                    .WithMany(item => item.EdgeNodes)
                    .HasForeignKey(item => item.EdgeId)
                    .IsRequired();
                edgeNode.HasOne(item => item.Node)
                    .WithMany(item => item.EdgeNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkDatabase>(networkDatabase =>
            {
                networkDatabase.HasKey(item => new { item.NetworkId, item.DatabaseId });
                networkDatabase.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkDatabases)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                networkDatabase.HasOne(item => item.Database)
                    .WithMany(item => item.NetworkDatabases)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkEdge>(networkEdge =>
            {
                networkEdge.HasKey(item => new { item.NetworkId, item.EdgeId });
                networkEdge.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkEdges)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                networkEdge.HasOne(item => item.Edge)
                    .WithMany(item => item.NetworkEdges)
                    .HasForeignKey(item => item.EdgeId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkNode>(networkNode =>
            {
                networkNode.HasKey(item => new { item.NetworkId, item.NodeId, item.Type });
                networkNode.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkNodes)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                networkNode.HasOne(item => item.Node)
                    .WithMany(item => item.NetworkNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkNodeCollection>(networkNodeCollection =>
            {
                networkNodeCollection.HasKey(item => new { item.NetworkId, item.NodeCollectionId, item.Type });
                networkNodeCollection.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkNodeCollections)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                networkNodeCollection.HasOne(item => item.NodeCollection)
                    .WithMany(item => item.NetworkNodeCollections)
                    .HasForeignKey(item => item.NodeCollectionId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkUser>(networkUser =>
            {
                networkUser.HasKey(item => new { item.NetworkId, item.Email });
                networkUser.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkUsers)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                networkUser.HasOne(item => item.User)
                    .WithMany(item => item.NetworkUsers)
                    .HasForeignKey(item => item.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<NodeCollectionNode>(nodeCollectionNode =>
            {
                nodeCollectionNode.HasKey(item => new { item.NodeCollectionId, item.NodeId });
                nodeCollectionNode.HasOne(item => item.NodeCollection)
                    .WithMany(item => item.NodeCollectionNodes)
                    .HasForeignKey(item => item.NodeCollectionId)
                    .IsRequired();
                nodeCollectionNode.HasOne(item => item.Node)
                    .WithMany(item => item.NodeCollectionNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
            modelBuilder.Entity<Path>(path =>
            {
                path.HasOne(item => item.ControlPath)
                    .WithMany(item => item.Paths)
                    .HasForeignKey(item => item.ControlPathId)
                    .IsRequired();
            });
            modelBuilder.Entity<PathEdge>(pathEdge =>
            {
                pathEdge.HasKey(item => new { item.PathId, item.EdgeId });
                pathEdge.HasOne(item => item.Path)
                    .WithMany(item => item.PathEdges)
                    .HasForeignKey(item => item.PathId)
                    .IsRequired();
                pathEdge.HasOne(item => item.Edge)
                    .WithMany(item => item.PathEdges)
                    .HasForeignKey(item => item.EdgeId)
                    .IsRequired();
            });
            modelBuilder.Entity<PathNode>(pathNode =>
            {
                pathNode.HasKey(item => new { item.PathId, item.NodeId, item.Type });
                pathNode.HasOne(item => item.Path)
                    .WithMany(item => item.PathNodes)
                    .HasForeignKey(item => item.PathId)
                    .IsRequired();
                pathNode.HasOne(item => item.Node)
                    .WithMany(item => item.PathNodes)
                    .HasForeignKey(item => item.NodeId)
                    .IsRequired();
            });
        }
    }
}
