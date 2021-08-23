using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents a navigation quick link.
    /// </summary>
    public class NavigationQuickLinkViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the quick link.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title in singular (displayed text) of the quick link.
        /// </summary>
        public string TitleSingular { get; set; }

        /// <summary>
        /// Gets or sets the title in plural (displayed text) of the quick link.
        /// </summary>
        public string TitlePlural { get; set; }

        /// <summary>
        /// Gets or sets the icon of the banner.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the color of the banner.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the width of the quick link.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the number of items of the quick link.
        /// </summary>
        public int? ItemCount { get; set; }

        /// <summary>
        /// Gets or sets the route ID of the quick link.
        /// </summary>
        public string RouteId { get; set; }

        /// <summary>
        /// Gets or sets the destination link of the quick link.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets the users navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AdministrationUsersNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Users",
            TitleSingular = "User",
            TitlePlural = "Users",
            Color = "success",
            Icon = "fa-user",
            Width = 6,
            Link = "/Administration/Accounts/Users/Index"
        };

        /// <summary>
        /// Gets the roles navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AdministrationRolesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Roles",
            TitleSingular = "Role",
            TitlePlural = "Roles",
            Color = "success",
            Icon = "fa-tag",
            Width = 6,
            Link = "/Administration/Accounts/Roles/Index"
        };

        /// <summary>
        /// Gets the databases navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AdministrationDatabasesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Databases",
            TitleSingular = "Database",
            TitlePlural = "Databases",
            Color = "info",
            Icon = "fa-database",
            Width = 6,
            Link = "/Administration/Databases/Databases/Index"
        };

        /// <summary>
        /// Gets the proteins navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AdministrationProteinsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Proteins",
            TitleSingular = "Protein",
            TitlePlural = "Proteins",
            Color = "primary",
            Icon = "fa-circle",
            Width = 6,
            Link = "/Administration/Data/Proteins/Index"
        };

        /// <summary>
        /// Gets the interactions navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AdministrationInteractionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Interactions",
            TitleSingular = "Interaction",
            TitlePlural = "Interactions",
            Color = "primary",
            Icon = "fa-arrow-right",
            Width = 6,
            Link = "/Administration/Data/Interactions/Index"
        };

        /// <summary>
        /// Gets the protein collections navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AdministrationProteinCollectionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "ProteinCollections",
            TitleSingular = "Collection",
            TitlePlural = "Collections",
            Color = "info",
            Icon = "fa-folder",
            Width = 6,
            Link = "/Administration/Data/ProteinCollections/Index"
        };

        /// <summary>
        /// Gets the proteins navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AdministrationNetworksNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Networks",
            TitleSingular = "Network",
            TitlePlural = "Networks",
            Color = "secondary",
            Icon = "fa-share-alt",
            Width = 6,
            Link = "/Administration/Created/Networks/Index"
        };

        /// <summary>
        /// Gets the interactions navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AdministrationAnalysesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Analyses",
            TitleSingular = "Analysis",
            TitlePlural = "Analyses",
            Color = "secondary",
            Icon = "fa-code-branch",
            Width = 6,
            Link = "/Administration/Created/Analyses/Index"
        };

        /// <summary>
        /// Gets the databases navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataDatabasesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Databases",
            TitleSingular = "Database",
            TitlePlural = "Databases",
            Color = "info",
            Icon = "fa-database",
            Width = 12,
            Link = "/AvailableData/Databases/Databases/Index"
        };

        /// <summary>
        /// Gets the proteins navigation quick link for the available data index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataProteinsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Proteins",
            TitleSingular = "Protein",
            TitlePlural = "Proteins",
            Color = "secondary",
            Icon = "fa-circle",
            Width = 12,
            Link = "/AvailableData/Data/Proteins/Index"
        };

        /// <summary>
        /// Gets the interactions navigation quick link for the available data index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataInteractionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Interactions",
            TitleSingular = "Interaction",
            TitlePlural = "Interactions",
            Color = "secondary",
            Icon = "fa-arrow-right",
            Width = 12,
            Link = "/AvailableData/Data/Interactions/Index"
        };

        /// <summary>
        /// Gets the protein collections navigation quick link for the available data index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataProteinCollectionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "ProteinCollections",
            TitleSingular = "Collection",
            TitlePlural = "Collections",
            Color = "info",
            Icon = "fa-folder",
            Width = 12,
            Link = "/AvailableData/Data/ProteinCollections/Index"
        };

        /// <summary>
        /// Gets the networks navigation quick link for the public data index page.
        /// </summary>
        public static NavigationQuickLinkViewModel PublicDataNetworksNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Networks",
            TitleSingular = "Network",
            TitlePlural = "Networks",
            Color = "success",
            Icon = "fa-share-alt",
            Width = 6,
            Link = "/PublicData/Networks/Index"
        };

        /// <summary>
        /// Gets the analyses navigation quick link for the public data index page.
        /// </summary>
        public static NavigationQuickLinkViewModel PublicDataAnalysesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Analyses",
            TitleSingular = "Analysis",
            TitlePlural = "Analyses",
            Color = "success",
            Icon = "fa-code-branch",
            Width = 6,
            Link = "/PublicData/Analyses/Index"
        };

        /// <summary>
        /// Gets the networks navigation quick link for the private data index page.
        /// </summary>
        public static NavigationQuickLinkViewModel PrivateDataNetworksNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Networks",
            TitleSingular = "Network",
            TitlePlural = "Networks",
            Color = "success",
            Icon = "fa-share-alt",
            Width = 6,
            Link = "/PrivateData/Networks/Index"
        };

        /// <summary>
        /// Gets the analyses navigation quick link for the private data index page.
        /// </summary>
        public static NavigationQuickLinkViewModel PrivateDataAnalysesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Analyses",
            TitleSingular = "Analysis",
            TitlePlural = "Analyses",
            Color = "success",
            Icon = "fa-code-branch",
            Width = 6,
            Link = "/PrivateData/Analyses/Index"
        };

        /// <summary>
        /// Gets the databases navigation quick link for the created data networks details page.
        /// </summary>
        public static NavigationQuickLinkViewModel CreatedDataNetworksDetailsDatabasesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Databases",
            TitleSingular = "Database",
            TitlePlural = "Databases",
            Color = "info",
            Icon = "fa-database",
            Width = 6,
            Link = "/CreatedData/Networks/Details/Databases/Databases/Index"
        };

        /// <summary>
        /// Gets the proteins navigation quick link for the created data networks details page.
        /// </summary>
        public static NavigationQuickLinkViewModel CreatedDataNetworksDetailsProteinsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Proteins",
            TitleSingular = "Protein",
            TitlePlural = "Proteins",
            Color = "success",
            Icon = "fa-circle",
            Width = 6,
            Link = "/CreatedData/Networks/Details/Data/Proteins/Index"
        };

        /// <summary>
        /// Gets the interactions navigation quick link for the created data networks details page.
        /// </summary>
        public static NavigationQuickLinkViewModel CreatedDataNetworksDetailsInteractionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Interactions",
            TitleSingular = "Interaction",
            TitlePlural = "Interactions",
            Color = "success",
            Icon = "fa-arrow-right",
            Width = 6,
            Link = "/CreatedData/Networks/Details/Data/Interactions/Index"
        };

        /// <summary>
        /// Gets the protein collections navigation quick link for the created data networks details page.
        /// </summary>
        public static NavigationQuickLinkViewModel CreatedDataNetworksDetailsProteinCollectionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "ProteinCollections",
            TitleSingular = "Collection",
            TitlePlural = "Collections",
            Color = "info",
            Icon = "fa-folder",
            Width = 6,
            Link = "/CreatedData/Networks/Details/Data/ProteinCollections/Index"
        };

        /// <summary>
        /// Gets the databases navigation quick link for the created data analyses details page.
        /// </summary>
        public static NavigationQuickLinkViewModel CreatedDataAnalysesDetailsDatabasesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Databases",
            TitleSingular = "Database",
            TitlePlural = "Databases",
            Color = "info",
            Icon = "fa-database",
            Width = 6,
            Link = "/CreatedData/Analyses/Details/Databases/Databases/Index"
        };

        /// <summary>
        /// Gets the proteins navigation quick link for the created data analyses details page.
        /// </summary>
        public static NavigationQuickLinkViewModel CreatedDataAnalysesDetailsProteinsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Proteins",
            TitleSingular = "Protein",
            TitlePlural = "Proteins",
            Color = "success",
            Icon = "fa-circle",
            Width = 6,
            Link = "/CreatedData/Analyses/Details/Data/Proteins/Index"
        };

        /// <summary>
        /// Gets the interactions navigation quick link for the created data analyses details page.
        /// </summary>
        public static NavigationQuickLinkViewModel CreatedDataAnalysesDetailsInteractionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Interactions",
            TitleSingular = "Interaction",
            TitlePlural = "Interactions",
            Color = "success",
            Icon = "fa-arrow-right",
            Width = 6,
            Link = "/CreatedData/Analyses/Details/Data/Interactions/Index"
        };

        /// <summary>
        /// Gets the protein collections navigation quick link for the created data analyses details page.
        /// </summary>
        public static NavigationQuickLinkViewModel CreatedDataAnalysesDetailsProteinCollectionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "ProteinCollections",
            TitleSingular = "Collection",
            TitlePlural = "Collections",
            Color = "info",
            Icon = "fa-folder",
            Width = 6,
            Link = "/CreatedData/Analyses/Details/Data/ProteinCollections/Index"
        };

        /// <summary>
        /// Gets the navigation quick links for the administration index page.
        /// </summary>
        /// <param name="count">The dictionary containing the current counts.</param>
        /// <returns>The navigation quick links for the content index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetAdministrationNavigationQuickLinks(Dictionary<string, int?> count = null)
        {
            // Get the corresponding navigation quick links.
            var usersNavigationQuickLink = AdministrationUsersNavigationQuickLink;
            var rolesNavigationQuickLink = AdministrationRolesNavigationQuickLink;
            var databasesNavigationQuickLink = AdministrationDatabasesNavigationQuickLink;
            var proteinCollectionsNavigationQuickLink = AdministrationProteinCollectionsNavigationQuickLink;
            var proteinsNavigationQuickLink = AdministrationProteinsNavigationQuickLink;
            var interactionsNavigationQuickLink = AdministrationInteractionsNavigationQuickLink;
            var networksNavigationQuickLink = AdministrationNetworksNavigationQuickLink;
            var analysesNavigationQuickLink = AdministrationAnalysesNavigationQuickLink;
            // Update the count and the route ID.
            usersNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Users", null);
            rolesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Roles", null);
            databasesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Databases", null);
            proteinCollectionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("ProteinCollections", null);
            proteinsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Proteins", null);
            interactionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Interactions", null);
            networksNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Networks", null);
            analysesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Analyses", null);
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                usersNavigationQuickLink,
                rolesNavigationQuickLink,
                databasesNavigationQuickLink,
                proteinCollectionsNavigationQuickLink,
                proteinsNavigationQuickLink,
                interactionsNavigationQuickLink,
                networksNavigationQuickLink,
                analysesNavigationQuickLink
            };
        }

        /// <summary>
        /// Gets the navigation quick links for the available data index page.
        /// </summary>
        /// <param name="count">The dictionary containing the current counts.</param>
        /// <returns>The navigation quick links for the content index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetAvailableDataNavigationQuickLinks(Dictionary<string, int?> count = null)
        {
            // Get the corresponding navigation quick links.
            var databasesNavigationQuickLink = AvailableDataDatabasesNavigationQuickLink;
            var proteinCollectionsNavigationQuickLink = AvailableDataProteinCollectionsNavigationQuickLink;
            var proteinsNavigationQuickLink = AvailableDataProteinsNavigationQuickLink;
            var interactionsNavigationQuickLink = AvailableDataInteractionsNavigationQuickLink;
            // Update the count and the route ID.
            databasesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Databases", null);
            proteinCollectionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("ProteinCollections", null);
            proteinsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Proteins", null);
            interactionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Interactions", null);
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                databasesNavigationQuickLink,
                proteinCollectionsNavigationQuickLink,
                proteinsNavigationQuickLink,
                interactionsNavigationQuickLink
            };
        }

        /// <summary>
        /// Gets the navigation quick links for the public data index page.
        /// </summary>
        /// <param name="count">The dictionary containing the current counts.</param>
        /// <returns>The navigation quick links for the content index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetPublicDataNavigationQuickLinks(Dictionary<string, int?> count = null)
        {
            // Get the corresponding navigation quick links.
            var networksNavigationQuickLink = PublicDataNetworksNavigationQuickLink;
            var analysesNavigationQuickLink = PublicDataAnalysesNavigationQuickLink;
            // Update the count and the route ID.
            networksNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Networks", null);
            analysesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Analyses", null);
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                networksNavigationQuickLink,
                analysesNavigationQuickLink
            };
        }

        /// <summary>
        /// Gets the navigation quick links for the public data index page.
        /// </summary>
        /// <param name="count">The dictionary containing the current counts.</param>
        /// <returns>The navigation quick links for the content index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetPrivateDataNavigationQuickLinks(Dictionary<string, int?> count = null)
        {
            // Get the corresponding navigation quick links.
            var networksNavigationQuickLink = PrivateDataNetworksNavigationQuickLink;
            var analysesNavigationQuickLink = PrivateDataAnalysesNavigationQuickLink;
            // Update the count and the route ID.
            networksNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Networks", null);
            analysesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Analyses", null);
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                networksNavigationQuickLink,
                analysesNavigationQuickLink
            };
        }

        /// <summary>
        /// Gets the navigation quick links for the created data networks details page.
        /// </summary>
        /// <param name="networkId">The ID of the current network.</param>
        /// <param name="count">The dictionary containing the current counts.</param>
        /// <returns>The navigation quick links for the network index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetCreatedDataNetworksDetailsNavigationQuickLinks(string networkId = null, Dictionary<string, int?> count = null)
        {
            // Get the corresponding navigation quick links.
            var databasesNavigationQuickLink = CreatedDataNetworksDetailsDatabasesNavigationQuickLink;
            var proteinsNavigationQuickLink = CreatedDataNetworksDetailsProteinsNavigationQuickLink;
            var interactionsNavigationQuickLink = CreatedDataNetworksDetailsInteractionsNavigationQuickLink;
            var proteinCollectionsNavigationQuickLink = CreatedDataNetworksDetailsProteinCollectionsNavigationQuickLink;
            // Update the count and the route ID.
            databasesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Databases", null);
            databasesNavigationQuickLink.RouteId = networkId;
            proteinsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Proteins", null);
            proteinsNavigationQuickLink.RouteId = networkId;
            interactionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Interactions", null);
            interactionsNavigationQuickLink.RouteId = networkId;
            proteinCollectionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("ProteinCollections", null);
            proteinCollectionsNavigationQuickLink.RouteId = networkId;
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                proteinsNavigationQuickLink,
                interactionsNavigationQuickLink,
                databasesNavigationQuickLink,
                proteinCollectionsNavigationQuickLink
            };
        }

        /// <summary>
        /// Gets the navigation quick links for the created data analyses details page.
        /// </summary>
        /// <param name="analysisId">The ID of the current analysis.</param>
        /// <param name="count">The dictionary containing the current counts.</param>
        /// <returns>The navigation quick links for the analysis index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetCreatedDataAnalysesDetailsNavigationQuickLinks(string analysisId = null, Dictionary<string, int?> count = null)
        {
            // Get the corresponding navigation quick links.
            var databasesNavigationQuickLink = CreatedDataAnalysesDetailsDatabasesNavigationQuickLink;
            var proteinsNavigationQuickLink = CreatedDataAnalysesDetailsProteinsNavigationQuickLink;
            var interactionsNavigationQuickLink = CreatedDataAnalysesDetailsInteractionsNavigationQuickLink;
            var proteinCollectionsNavigationQuickLink = CreatedDataAnalysesDetailsProteinCollectionsNavigationQuickLink;
            // Update the count and the route ID.
            databasesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Databases", null);
            databasesNavigationQuickLink.RouteId = analysisId;
            proteinsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Proteins", null);
            proteinsNavigationQuickLink.RouteId = analysisId;
            interactionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Interactions", null);
            interactionsNavigationQuickLink.RouteId = analysisId;
            proteinCollectionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("ProteinCollections", null);
            proteinCollectionsNavigationQuickLink.RouteId = analysisId;
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                proteinsNavigationQuickLink,
                interactionsNavigationQuickLink,
                databasesNavigationQuickLink,
                proteinCollectionsNavigationQuickLink
            };
        }
    }
}
