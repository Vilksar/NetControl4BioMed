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
        /// Gets or sets the title (displayed text) of the quick link.
        /// </summary>
        public string Title { get; set; }

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
            Title = "User",
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
            Title = "Role",
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
            Title = "Database",
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
            Title = "Protein",
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
            Title = "Interaction",
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
            Title = "Protein collection",
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
            Title = "Network",
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
            Title = "Analysis",
            Color = "secondary",
            Icon = "fa-desktop",
            Width = 6,
            Link = "/Administration/Created/Analyses/Index"
        };

        /// <summary>
        /// Gets the networks navigation quick link for the public data index page.
        /// </summary>
        public static NavigationQuickLinkViewModel PublicDataNetworksNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Networks",
            Title = "Network",
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
            Title = "Analysis",
            Color = "success",
            Icon = "fa-desktop",
            Width = 6,
            Link = "/PrivateData/Analyses/Index"
        };

        /// <summary>
        /// Gets the networks navigation quick link for the private data index page.
        /// </summary>
        public static NavigationQuickLinkViewModel PrivateDataNetworksNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Networks",
            Title = "Network",
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
            Title = "Analysis",
            Color = "success",
            Icon = "fa-desktop",
            Width = 6,
            Link = "/PublicData/Analyses/Index"
        };

        /// <summary>
        /// Gets the databases navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataDatabasesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Databases",
            Title = "Database",
            Color = "info",
            Icon = "fa-database",
            Width = 6,
            Link = "/AvailableData/Databases/Databases/Index"
        };

        /// <summary>
        /// Gets the proteins navigation quick link for the available data index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataProteinsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Proteins",
            Title = "Protein",
            Color = "primary",
            Icon = "fa-circle",
            Width = 6,
            Link = "/AvailableData/Data/Proteins/Index"
        };

        /// <summary>
        /// Gets the interactions navigation quick link for the available data index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataInteractionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Interactions",
            Title = "Interaction",
            Color = "primary",
            Icon = "fa-arrow-right",
            Width = 6,
            Link = "/AvailableData/Data/Interactions/Index"
        };

        /// <summary>
        /// Gets the protein collections navigation quick link for the available data index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataProteinCollectionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "ProteinCollections",
            Title = "Protein collection",
            Color = "info",
            Icon = "fa-folder",
            Width = 6,
            Link = "/AvailableData/Data/ProteinCollections/Index"
        };

        /// <summary>
        /// Gets the proteins navigation quick link for the networks index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataNetworkProteinsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Proteins",
            Title = "Protein",
            Color = "success",
            Icon = "fa-circle",
            Width = 6,
            Link = "/AvailableData/Created/Networks/Details/Data/Proteins/Index"
        };

        /// <summary>
        /// Gets the interactions navigation quick link for the networks index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataNetworkInteractionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Interactions",
            Title = "Interaction",
            Color = "success",
            Icon = "fa-arrow-right",
            Width = 6,
            Link = "/AvailableData/Created/Networks/Details/Data/Interactions/Index"
        };

        /// <summary>
        /// Gets the databases navigation quick link for the networks index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataNetworkDatabasesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Databases",
            Title = "Database",
            Color = "info",
            Icon = "fa-database",
            Width = 6,
            Link = "/AvailableData/Created/Networks/Details/Databases/Databases/Index"
        };

        /// <summary>
        /// Gets the protein collections navigation quick link for the networks index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataNetworkProteinCollectionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "ProteinCollections",
            Title = "Protein collection",
            Color = "info",
            Icon = "fa-folder",
            Width = 6,
            Link = "/AvailableData/Created/Networks/Details/Data/ProteinCollections/Index"
        };

        /// <summary>
        /// Gets the users navigation quick link for the networks index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataNetworkUsersNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Users",
            Title = "User",
            Color = "primary",
            Icon = "fa-user",
            Width = 6,
            Link = "/AvailableData/Created/Networks/Details/Accounts/Users/Index"
        };

        /// <summary>
        /// Gets the analyses navigation quick link for the networks index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataNetworkAnalysesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Analyses",
            Title = "Analysis",
            Color = "primary",
            Icon = "fa-desktop",
            Width = 6,
            Link = "/AvailableData/Created/Networks/Details/Created/Analyses/Index"
        };

        /// <summary>
        /// Gets the proteins navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataAnalysisProteinsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Proteins",
            Title = "Protein",
            Color = "success",
            Icon = "fa-circle",
            Width = 6,
            Link = "/AvailableData/Created/Analyses/Details/Data/Proteins/Index"
        };

        /// <summary>
        /// Gets the interactions navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataAnalysisInteractionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Interactions",
            Title = "Interaction",
            Color = "success",
            Icon = "fa-arrow-right",
            Width = 6,
            Link = "/AvailableData/Created/Analyses/Details/Data/Interactions/Index"
        };

        /// <summary>
        /// Gets the databases navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataAnalysisDatabasesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Databases",
            Title = "Database",
            Color = "info",
            Icon = "fa-database",
            Width = 6,
            Link = "/AvailableData/Created/Analyses/Details/Databases/Databases/Index"
        };

        /// <summary>
        /// Gets the protein collections navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataAnalysisProteinCollectionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "ProteinCollections",
            Title = "Protein collection",
            Color = "info",
            Icon = "fa-folder",
            Width = 6,
            Link = "/AvailableData/Created/Analyses/Details/Data/ProteinCollections/Index"
        };

        /// <summary>
        /// Gets the users navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataAnalysisUsersNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Users",
            Title = "User",
            Color = "primary",
            Icon = "fa-user",
            Width = 6,
            Link = "/AvailableData/Created/Analyses/Details/Accounts/Users/Index"
        };

        /// <summary>
        /// Gets the networks navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AvailableDataAnalysisControlPathsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "ControlPaths",
            Title = "Control paths",
            Color = "primary",
            Icon = "fa-gamepad",
            Width = 6,
            Link = "/AvailableData/Created/Analyses/Details/Results/ControlPaths/Index"
        };

        /// <summary>
        /// Gets the navigation quick links for the administration index page.
        /// </summary>
        /// <param name="count">The dictionary containing the current counts.</param>
        /// <returns>The navigation quick links for the content index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetAdministrationNavigationQuickLinks(Dictionary<string, int?> count = null)
        {
            // Get the corresponding navigation quick links.
            var administrationUsersNavigationQuickLink = AdministrationUsersNavigationQuickLink;
            var administrationRolesNavigationQuickLink = AdministrationRolesNavigationQuickLink;
            var administrationDatabasesNavigationQuickLink = AdministrationDatabasesNavigationQuickLink;
            var administrationProteinCollectionsNavigationQuickLink = AdministrationProteinCollectionsNavigationQuickLink;
            var administrationProteinsNavigationQuickLink = AdministrationProteinsNavigationQuickLink;
            var administrationInteractionsNavigationQuickLink = AdministrationInteractionsNavigationQuickLink;
            var administrationNetworksNavigationQuickLink = AdministrationNetworksNavigationQuickLink;
            var administrationAnalysesNavigationQuickLink = AdministrationAnalysesNavigationQuickLink;
            // Update the count and the route ID.
            administrationUsersNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Users", null);
            administrationRolesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Roles", null);
            administrationDatabasesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Databases", null);
            administrationProteinCollectionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("ProteinCollections", null);
            administrationProteinsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Proteins", null);
            administrationInteractionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Interactions", null);
            administrationNetworksNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Networks", null);
            administrationAnalysesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Analyses", null);
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                administrationUsersNavigationQuickLink,
                administrationRolesNavigationQuickLink,
                administrationDatabasesNavigationQuickLink,
                administrationProteinCollectionsNavigationQuickLink,
                administrationProteinsNavigationQuickLink,
                administrationInteractionsNavigationQuickLink,
                administrationNetworksNavigationQuickLink,
                administrationAnalysesNavigationQuickLink
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
            var publicDataNetworksNavigationQuickLink = PublicDataNetworksNavigationQuickLink;
            var publicDataAnalysesNavigationQuickLink = PublicDataAnalysesNavigationQuickLink;
            // Update the count and the route ID.
            publicDataNetworksNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Networks", null);
            publicDataAnalysesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Analyses", null);
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                publicDataNetworksNavigationQuickLink,
                publicDataAnalysesNavigationQuickLink
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
            var privateDataNetworksNavigationQuickLink = PrivateDataNetworksNavigationQuickLink;
            var privateDataAnalysesNavigationQuickLink = PrivateDataAnalysesNavigationQuickLink;
            // Update the count and the route ID.
            privateDataNetworksNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Networks", null);
            privateDataAnalysesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Analyses", null);
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                privateDataNetworksNavigationQuickLink,
                privateDataAnalysesNavigationQuickLink
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
            var availableDataDatabasesNavigationQuickLink = AvailableDataDatabasesNavigationQuickLink;
            var availableDataProteinCollectionsNavigationQuickLink = AvailableDataProteinCollectionsNavigationQuickLink;
            var availableDataProteinsNavigationQuickLink = AvailableDataProteinsNavigationQuickLink;
            var availableDataInteractionsNavigationQuickLink = AvailableDataInteractionsNavigationQuickLink;
            // Update the count and the route ID.
            availableDataDatabasesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Databases", null);
            availableDataProteinCollectionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("ProteinCollections", null);
            availableDataProteinsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Proteins", null);
            availableDataInteractionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Interactions", null);
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                availableDataDatabasesNavigationQuickLink,
                availableDataProteinCollectionsNavigationQuickLink,
                availableDataProteinsNavigationQuickLink,
                availableDataInteractionsNavigationQuickLink
            };
        }

        /// <summary>
        /// Gets the navigation quick links for the network index page.
        /// </summary>
        /// <param name="networkId">The ID of the current network.</param>
        /// <param name="count">The dictionary containing the current counts.</param>
        /// <returns>The navigation quick links for the network index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetAvailableDataNetworkNavigationQuickLinks(string networkId = null, Dictionary<string, int?> count = null)
        {
            // Check if there is no network ID provided.
            if (string.IsNullOrEmpty(networkId))
            {
                // Assign the empty string to it.
                networkId = string.Empty;
            }
            // Get the corresponding navigation quick links.
            var networkProteinsNavigationQuickLink = AvailableDataNetworkProteinsNavigationQuickLink;
            var networkInteractionsNavigationQuickLink = AvailableDataNetworkInteractionsNavigationQuickLink;
            var networkDatabasesNavigationQuickLink = AvailableDataNetworkDatabasesNavigationQuickLink;
            var networkProteinCollectionsNavigationQuickLink = AvailableDataNetworkProteinCollectionsNavigationQuickLink;
            var networkUsersNavigationQuickLink = AvailableDataNetworkUsersNavigationQuickLink;
            var networkAnalysesNavigationQuickLink = AvailableDataNetworkAnalysesNavigationQuickLink;
            // Update the count and the route ID.
            networkProteinsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Proteins", null);
            networkProteinsNavigationQuickLink.RouteId = networkId;
            networkInteractionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Interactions", null);
            networkInteractionsNavigationQuickLink.RouteId = networkId;
            networkDatabasesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Databases", null);
            networkDatabasesNavigationQuickLink.RouteId = networkId;
            networkProteinCollectionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("ProteinCollections", null);
            networkProteinCollectionsNavigationQuickLink.RouteId = networkId;
            networkUsersNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Users", null);
            networkUsersNavigationQuickLink.RouteId = networkId;
            networkAnalysesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Analyses", null);
            networkAnalysesNavigationQuickLink.RouteId = networkId;
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                networkProteinsNavigationQuickLink,
                networkInteractionsNavigationQuickLink,
                networkDatabasesNavigationQuickLink,
                networkProteinCollectionsNavigationQuickLink,
                networkUsersNavigationQuickLink,
                networkAnalysesNavigationQuickLink
            };
        }

        /// <summary>
        /// Gets the navigation quick links for the analysis index page.
        /// </summary>
        /// <param name="analysisId">The ID of the current analysis.</param>
        /// <param name="count">The dictionary containing the current counts.</param>
        /// <returns>The navigation quick links for the analysis index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetAvailableDataAnalysisNavigationQuickLinks(string analysisId = null, Dictionary<string, int?> count = null)
        {
            // Check if there is no analysis ID provided.
            if (string.IsNullOrEmpty(analysisId))
            {
                // Assign the empty string to it.
                analysisId = string.Empty;
            }
            // Get the corresponding navigation quick links.
            var analysisProteinsNavigationQuickLink = AvailableDataAnalysisProteinsNavigationQuickLink;
            var analysisInteractionsNavigationQuickLink = AvailableDataAnalysisInteractionsNavigationQuickLink;
            var analysisDatabasesNavigationQuickLink = AvailableDataAnalysisDatabasesNavigationQuickLink;
            var analysisProteinCollectionsNavigationQuickLink = AvailableDataAnalysisProteinCollectionsNavigationQuickLink;
            var analysisUsersNavigationQuickLink = AvailableDataAnalysisUsersNavigationQuickLink;
            var analysisControlPathsNavigationQuickLink = AvailableDataAnalysisControlPathsNavigationQuickLink;
            // Update the count and the route ID.
            analysisProteinsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Proteins", null);
            analysisProteinsNavigationQuickLink.RouteId = analysisId;
            analysisInteractionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Interactions", null);
            analysisInteractionsNavigationQuickLink.RouteId = analysisId;
            analysisDatabasesNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Databases", null);
            analysisDatabasesNavigationQuickLink.RouteId = analysisId;
            analysisProteinCollectionsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("ProteinCollections", null);
            analysisProteinCollectionsNavigationQuickLink.RouteId = analysisId;
            analysisUsersNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Users", null);
            analysisUsersNavigationQuickLink.RouteId = analysisId;
            analysisControlPathsNavigationQuickLink.ItemCount = count?.GetValueOrDefault("Networks", null);
            analysisControlPathsNavigationQuickLink.RouteId = analysisId;
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                analysisProteinsNavigationQuickLink,
                analysisInteractionsNavigationQuickLink,
                analysisDatabasesNavigationQuickLink,
                analysisProteinCollectionsNavigationQuickLink,
                analysisUsersNavigationQuickLink,
                analysisControlPathsNavigationQuickLink
            };
        }
    }
}
