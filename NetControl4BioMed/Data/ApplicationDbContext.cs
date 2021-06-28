using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Data
{/// <summary>
 /// Represents the database context of the application.
 /// </summary>
    public class ApplicationDbContext : IdentityDbContext<User, Role, string, IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        /// <summary>
        /// Represents the default batch size for a database operation.
        /// </summary>
        public static int BatchSize { get; } = 200;

        /// <summary>
        /// Gets or sets the number of days before user-created database items will be automatically stopped.
        /// </summary>
        public static int DaysBeforeStop { get; } = 1;

        /// <summary>
        /// Gets or sets the number of days before an alert on user-created database items close to deletion will be automatically sent.
        /// </summary>
        public static int DaysBeforeAlert { get; } = 83;

        /// <summary>
        /// Gets or sets the number of days before user-created database items will be automatically deleted.
        /// </summary>
        public static int DaysBeforeDelete { get; } = 90;

        /// <summary>
        /// Gets or sets the database table containing the analyses.
        /// </summary>
        public DbSet<Analysis> Analyses { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between analyses and databases.
        /// </summary>
        public DbSet<AnalysisDatabase> AnalysisDatabases { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between analyses and interactions.
        /// </summary>
        public DbSet<AnalysisInteraction> AnalysisInteractions { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between analyses and proteins.
        /// </summary>
        public DbSet<AnalysisProtein> AnalysisProteins { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between analyses and protein collections.
        /// </summary>
        public DbSet<AnalysisProteinCollection> AnalysisProteinCollections { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between analyses and users.
        /// </summary>
        public DbSet<AnalysisUser> AnalysisUsers { get; set; }

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
        /// Gets or sets the database table containing the one-to-one relationship between databases and interactions.
        /// </summary>
        public DbSet<DatabaseInteraction> DatabaseInteractions { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the database interaction fields.
        /// </summary>
        public DbSet<DatabaseInteractionField> DatabaseInteractionFields { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between database interaction fields and interactions.
        /// </summary>
        public DbSet<DatabaseInteractionFieldInteraction> DatabaseInteractionFieldInteractions { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between databases and proteins.
        /// </summary>
        public DbSet<DatabaseProtein> DatabaseProteins { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the database protein fields.
        /// </summary>
        public DbSet<DatabaseProteinField> DatabaseProteinFields { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between database protein fields and proteins.
        /// </summary>
        public DbSet<DatabaseProteinFieldProtein> DatabaseProteinFieldProteins { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between databases and users.
        /// </summary>
        public DbSet<DatabaseUser> DatabaseUsers { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the interactions.
        /// </summary>
        public DbSet<Interaction> Interactions { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between interactions and proteins.
        /// </summary>
        public DbSet<InteractionProtein> InteractionProteins { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the networks.
        /// </summary>
        public DbSet<Network> Networks { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between networks and databases.
        /// </summary>
        public DbSet<NetworkDatabase> NetworkDatabases { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between networks and interactions.
        /// </summary>
        public DbSet<NetworkInteraction> NetworkInteractions { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between networks and proteins.
        /// </summary>
        public DbSet<NetworkProtein> NetworkProteins { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between networks and protein collections.
        /// </summary>
        public DbSet<NetworkProteinCollection> NetworkProteinCollections { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between networks and users.
        /// </summary>
        public DbSet<NetworkUser> NetworkUsers { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the proteins.
        /// </summary>
        public DbSet<Protein> Proteins { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the protein collections.
        /// </summary>
        public DbSet<ProteinCollection> ProteinCollections { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between protein collections and proteins.
        /// </summary>
        public DbSet<ProteinCollectionProtein> ProteinCollectionProteins { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between protein collections and types.
        /// </summary>
        public DbSet<ProteinCollectionType> ProteinCollectionTypes { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the paths in control paths for analyses.
        /// </summary>
        public DbSet<Path> Paths { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between paths and interactions.
        /// </summary>
        public DbSet<PathInteraction> PathInteractions { get; set; }

        /// <summary>
        /// Gets or sets the database table containing the one-to-one relationship between paths and proteins.
        /// </summary>
        public DbSet<PathProtein> PathProteins { get; set; }

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
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.Analyses)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisDatabase>(entity =>
            {
                entity.HasKey(item => new { item.AnalysisId, item.DatabaseId, item.Type });
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisDatabases)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.AnalysisDatabases)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisInteraction>(entity =>
            {
                entity.HasKey(item => new { item.AnalysisId, item.InteractionId });
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisInteractions)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                entity.HasOne(item => item.Interaction)
                    .WithMany(item => item.AnalysisInteractions)
                    .HasForeignKey(item => item.InteractionId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisProtein>(entity =>
            {
                entity.HasKey(item => new { item.AnalysisId, item.ProteinId, item.Type });
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisProteins)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                entity.HasOne(item => item.Protein)
                    .WithMany(item => item.AnalysisProteins)
                    .HasForeignKey(item => item.ProteinId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisProteinCollection>(entity =>
            {
                entity.HasKey(item => new { item.AnalysisId, item.ProteinCollectionId, item.Type });
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisProteinCollections)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                entity.HasOne(item => item.ProteinCollection)
                    .WithMany(item => item.AnalysisProteinCollections)
                    .HasForeignKey(item => item.ProteinCollectionId)
                    .IsRequired();
            });
            modelBuilder.Entity<AnalysisUser>(entity =>
            {
                entity.HasKey(item => new { item.AnalysisId, item.Email });
                entity.HasOne(item => item.Analysis)
                    .WithMany(item => item.AnalysisUsers)
                    .HasForeignKey(item => item.AnalysisId)
                    .IsRequired();
                entity.HasOne(item => item.User)
                    .WithMany(item => item.AnalysisUsers)
                    .HasForeignKey(item => item.UserId);
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
            });
            modelBuilder.Entity<DatabaseInteraction>(entity =>
            {
                entity.HasKey(item => new { item.DatabaseId, item.InteractionId });
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseInteractions)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
                entity.HasOne(item => item.Interaction)
                    .WithMany(item => item.DatabaseInteractions)
                    .HasForeignKey(item => item.InteractionId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseInteractionField>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseInteractionFields)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseInteractionFieldInteraction>(entity =>
            {
                entity.HasKey(item => new { item.DatabaseInteractionFieldId, item.InteractionId, item.Value });
                entity.HasOne(item => item.DatabaseInteractionField)
                    .WithMany(item => item.DatabaseInteractionFieldInteractions)
                    .HasForeignKey(item => item.DatabaseInteractionFieldId)
                    .IsRequired();
                entity.HasOne(item => item.Interaction)
                    .WithMany(item => item.DatabaseInteractionFieldInteractions)
                    .HasForeignKey(item => item.InteractionId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseProtein>(entity =>
            {
                entity.HasKey(item => new { item.DatabaseId, item.ProteinId });
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseProteins)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
                entity.HasOne(item => item.Protein)
                    .WithMany(item => item.DatabaseProteins)
                    .HasForeignKey(item => item.ProteinId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseProteinField>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseProteinFields)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseProteinFieldProtein>(entity =>
            {
                entity.HasKey(item => new { item.DatabaseProteinFieldId, item.ProteinId, item.Value });
                entity.HasOne(item => item.DatabaseProteinField)
                    .WithMany(item => item.DatabaseProteinFieldProteins)
                    .HasForeignKey(item => item.DatabaseProteinFieldId)
                    .IsRequired();
                entity.HasOne(item => item.Protein)
                    .WithMany(item => item.DatabaseProteinFieldProteins)
                    .HasForeignKey(item => item.ProteinId)
                    .IsRequired();
            });
            modelBuilder.Entity<DatabaseUser>(entity =>
            {
                entity.HasKey(item => new { item.DatabaseId, item.Email });
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.DatabaseUsers)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
                entity.HasOne(item => item.User)
                    .WithMany(item => item.DatabaseUsers)
                    .HasForeignKey(item => item.UserId);
            });
            modelBuilder.Entity<Interaction>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<InteractionProtein>(entity =>
            {
                entity.HasKey(item => new { item.InteractionId, item.ProteinId, item.Type });
                entity.HasOne(item => item.Interaction)
                    .WithMany(item => item.InteractionProteins)
                    .HasForeignKey(item => item.InteractionId)
                    .IsRequired();
                entity.HasOne(item => item.Protein)
                    .WithMany(item => item.InteractionProteins)
                    .HasForeignKey(item => item.ProteinId)
                    .IsRequired();
            });
            modelBuilder.Entity<Network>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<NetworkDatabase>(entity =>
            {
                entity.HasKey(item => new { item.NetworkId, item.DatabaseId, item.Type });
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkDatabases)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                entity.HasOne(item => item.Database)
                    .WithMany(item => item.NetworkDatabases)
                    .HasForeignKey(item => item.DatabaseId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkInteraction>(entity =>
            {
                entity.HasKey(item => new { item.NetworkId, item.InteractionId });
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkInteractions)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                entity.HasOne(item => item.Interaction)
                    .WithMany(item => item.NetworkInteractions)
                    .HasForeignKey(item => item.InteractionId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkProtein>(entity =>
            {
                entity.HasKey(item => new { item.NetworkId, item.ProteinId, item.Type });
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkProteins)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                entity.HasOne(item => item.Protein)
                    .WithMany(item => item.NetworkProteins)
                    .HasForeignKey(item => item.ProteinId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkProteinCollection>(entity =>
            {
                entity.HasKey(item => new { item.NetworkId, item.ProteinCollectionId, item.Type });
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkProteinCollections)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                entity.HasOne(item => item.ProteinCollection)
                    .WithMany(item => item.NetworkProteinCollections)
                    .HasForeignKey(item => item.ProteinCollectionId)
                    .IsRequired();
            });
            modelBuilder.Entity<NetworkUser>(entity =>
            {
                entity.HasKey(item => new { item.NetworkId, item.Email });
                entity.HasOne(item => item.Network)
                    .WithMany(item => item.NetworkUsers)
                    .HasForeignKey(item => item.NetworkId)
                    .IsRequired();
                entity.HasOne(item => item.User)
                    .WithMany(item => item.NetworkUsers)
                    .HasForeignKey(item => item.UserId);
            });
            modelBuilder.Entity<Protein>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<ProteinCollection>(entity =>
            {
                entity.Property(item => item.Id)
                    .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<ProteinCollectionProtein>(entity =>
            {
                entity.HasKey(item => new { item.ProteinCollectionId, item.ProteinId });
                entity.HasOne(item => item.ProteinCollection)
                    .WithMany(item => item.ProteinCollectionProteins)
                    .HasForeignKey(item => item.ProteinCollectionId)
                    .IsRequired();
                entity.HasOne(item => item.Protein)
                    .WithMany(item => item.ProteinCollectionProteins)
                    .HasForeignKey(item => item.ProteinId)
                    .IsRequired();
            });
            modelBuilder.Entity<ProteinCollectionType>(entity =>
            {
                entity.HasKey(item => new { item.ProteinCollectionId, item.Type });
                entity.HasOne(item => item.ProteinCollection)
                    .WithMany(item => item.ProteinCollectionTypes)
                    .HasForeignKey(item => item.ProteinCollectionId)
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
            modelBuilder.Entity<PathInteraction>(entity =>
            {
                entity.HasKey(item => new { item.PathId, item.InteractionId, item.Index });
                entity.HasOne(item => item.Path)
                    .WithMany(item => item.PathInteractions)
                    .HasForeignKey(item => item.PathId)
                    .IsRequired();
                entity.HasOne(item => item.Interaction)
                    .WithMany(item => item.PathInteractions)
                    .HasForeignKey(item => item.InteractionId)
                    .IsRequired();
            });
            modelBuilder.Entity<PathProtein>(entity =>
            {
                entity.HasKey(item => new { item.PathId, item.ProteinId, item.Type, item.Index });
                entity.HasOne(item => item.Path)
                    .WithMany(item => item.PathProteins)
                    .HasForeignKey(item => item.PathId)
                    .IsRequired();
                entity.HasOne(item => item.Protein)
                    .WithMany(item => item.PathProteins)
                    .HasForeignKey(item => item.ProteinId)
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
