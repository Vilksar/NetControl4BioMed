using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public int ItemCount { get; set; }

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
        /// Gets the node collections navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AdministrationNodeCollectionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "NodeCollections",
            Title = "Node collection",
            Color = "info",
            Icon = "fa-folder",
            Width = 6,
            Link = "/Administration/Data/NodeCollections/Index"
        };

        /// <summary>
        /// Gets the nodes navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AdministrationNodesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Nodes",
            Title = "Node",
            Color = "primary",
            Icon = "fa-circle",
            Width = 6,
            Link = "/Administration/Data/Nodes/Index"
        };

        /// <summary>
        /// Gets the edges navigation quick link for the administration index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AdministrationEdgesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Edges",
            Title = "Edge",
            Color = "primary",
            Icon = "fa-arrow-right",
            Width = 6,
            Link = "/Administration/Data/Edges/Index"
        };

        /// <summary>
        /// Gets the networks navigation quick link for the content index page.
        /// </summary>
        public static NavigationQuickLinkViewModel ContentNetworksNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Networks",
            Title = "Network",
            Color = "info",
            Icon = "fa-share-alt",
            Width = 6,
            Link = "/Content/Created/Networks/Index"
        };

        /// <summary>
        /// Gets the analyses navigation quick link for the content index page.
        /// </summary>
        public static NavigationQuickLinkViewModel ContentAnalysesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Analyses",
            Title = "Analysis",
            Color = "info",
            Icon = "fa-desktop",
            Width = 6,
            Link = "/Content/Created/Analyses/Index"
        };

        /// <summary>
        /// Gets the nodes navigation quick link for the networks index page.
        /// </summary>
        public static NavigationQuickLinkViewModel NetworkNodesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Nodes",
            Title = "Node",
            Color = "success",
            Icon = "fa-circle",
            Width = 6,
            Link = "/Content/Created/Networks/Details/Data/Nodes/Index"
        };

        /// <summary>
        /// Gets the edges navigation quick link for the networks index page.
        /// </summary>
        public static NavigationQuickLinkViewModel NetworkEdgesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Edges",
            Title = "Edge",
            Color = "success",
            Icon = "fa-arrow-right",
            Width = 6,
            Link = "/Content/Created/Networks/Details/Data/Edges/Index"
        };

        /// <summary>
        /// Gets the databases navigation quick link for the networks index page.
        /// </summary>
        public static NavigationQuickLinkViewModel NetworkDatabasesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Databases",
            Title = "Database",
            Color = "info",
            Icon = "fa-database",
            Width = 6,
            Link = "/Content/Created/Networks/Details/Databases/Databases/Index"
        };

        /// <summary>
        /// Gets the node collections navigation quick link for the networks index page.
        /// </summary>
        public static NavigationQuickLinkViewModel NetworkNodeCollectionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "NodeCollections",
            Title = "Node collection",
            Color = "info",
            Icon = "fa-folder",
            Width = 6,
            Link = "/Content/Created/Networks/Details/Data/NodeCollections/Index"
        };

        /// <summary>
        /// Gets the users navigation quick link for the networks index page.
        /// </summary>
        public static NavigationQuickLinkViewModel NetworkUsersNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Users",
            Title = "User",
            Color = "primary",
            Icon = "fa-user",
            Width = 6,
            Link = "/Content/Created/Networks/Details/Accounts/Users/Index"
        };

        /// <summary>
        /// Gets the user invitations navigation quick link for the networks index page.
        /// </summary>
        public static NavigationQuickLinkViewModel NetworkUserInvitationsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "UserInvitations",
            Title = "User invitations",
            Color = "primary",
            Icon = "fa-envelope-open",
            Width = 6,
            Link = "/Content/Created/Networks/Details/Accounts/Users/Index"
        };

        /// <summary>
        /// Gets the nodes navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AnalysisNodesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Nodes",
            Title = "Node",
            Color = "success",
            Icon = "fa-circle",
            Width = 6,
            Link = "/Content/Created/Analyses/Details/Data/Nodes/Index"
        };

        /// <summary>
        /// Gets the edges navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AnalysisEdgesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Edges",
            Title = "Edge",
            Color = "success",
            Icon = "fa-arrow-right",
            Width = 6,
            Link = "/Content/Created/Analyses/Details/Data/Edges/Index"
        };

        /// <summary>
        /// Gets the databases navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AnalysisDatabasesNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Databases",
            Title = "Database",
            Color = "info",
            Icon = "fa-database",
            Width = 6,
            Link = "/Content/Created/Analyses/Details/Databases/Databases/Index"
        };

        /// <summary>
        /// Gets the node collections navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AnalysisNodeCollectionsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "NodeCollections",
            Title = "Node collection",
            Color = "info",
            Icon = "fa-folder",
            Width = 6,
            Link = "/Content/Created/Analyses/Details/Data/NodeCollections/Index"
        };

        /// <summary>
        /// Gets the users navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AnalysisUsersNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "Users",
            Title = "User",
            Color = "primary",
            Icon = "fa-user",
            Width = 6,
            Link = "/Content/Created/Analyses/Details/Accounts/Users/Index"
        };

        /// <summary>
        /// Gets the user invitations navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AnalysisUserInvitationsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "UserInvitations",
            Title = "User invitation",
            Color = "primary",
            Icon = "fa-envelope-open",
            Width = 6,
            Link = "/Content/Created/Analyses/Details/Accounts/Users/Index"
        };

        /// <summary>
        /// Gets the navigation quick links for the administration index page.
        /// </summary>
        /// <param name="userCount">The current number of users.</param>
        /// <param name="roleCount">The current number of roles.</param>
        /// <param name="databaseCount">The current number of databases.</param>
        /// <param name="nodeCollectionCount">The current number of node collections.</param>
        /// <param name="nodeCount">The current number of nodes.</param>
        /// <param name="edgeCount">The current number of edges.</param>
        /// <returns>The navigation quick links for the content index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetAdministrationNavigationQuickLinks(int userCount = 0, int roleCount = 0, int databaseCount = 0, int nodeCollectionCount = 0, int nodeCount = 0, int edgeCount = 0)
        {
            // Get the corresponding navigation quick links.
            var administrationUsersNavigationQuickLink = AdministrationUsersNavigationQuickLink;
            var administrationRolesNavigationQuickLink = AdministrationRolesNavigationQuickLink;
            var administrationDatabasesNavigationQuickLink = AdministrationDatabasesNavigationQuickLink;
            var administrationNodeCollectionsNavigationQuickLink = AdministrationNodeCollectionsNavigationQuickLink;
            var administrationNodesNavigationQuickLink = AdministrationNodesNavigationQuickLink;
            var administrationEdgesNavigationQuickLink = AdministrationEdgesNavigationQuickLink;
            // Update the count and the route ID.
            administrationUsersNavigationQuickLink.ItemCount = userCount;
            administrationRolesNavigationQuickLink.ItemCount = roleCount;
            administrationDatabasesNavigationQuickLink.ItemCount = databaseCount;
            administrationNodeCollectionsNavigationQuickLink.ItemCount = nodeCollectionCount;
            administrationNodesNavigationQuickLink.ItemCount = nodeCount;
            administrationEdgesNavigationQuickLink.ItemCount = edgeCount;
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                administrationUsersNavigationQuickLink,
                administrationRolesNavigationQuickLink,
                administrationDatabasesNavigationQuickLink,
                administrationNodeCollectionsNavigationQuickLink,
                administrationNodesNavigationQuickLink,
                administrationEdgesNavigationQuickLink
            };
        }

        /// <summary>
        /// Gets the navigation quick links for the content index page.
        /// </summary>
        /// <param name="networkCount">The current number of networks.</param>
        /// <param name="analysisCount">The current number of analyses.</param>
        /// <returns>The navigation quick links for the content index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetContentNavigationQuickLinks(int networkCount = 0, int analysisCount = 0)
        {
            // Get the corresponding navigation quick links.
            var contentNetworksNavigationQuickLink = ContentNetworksNavigationQuickLink;
            var contentAnalysesNavigationQuickLink = ContentAnalysesNavigationQuickLink;
            // Update the count and the route ID.
            contentNetworksNavigationQuickLink.ItemCount = networkCount;
            contentAnalysesNavigationQuickLink.ItemCount = analysisCount;
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                contentNetworksNavigationQuickLink,
                contentAnalysesNavigationQuickLink
            };
        }

        /// <summary>
        /// Gets the navigation quick links for the network index page.
        /// </summary>
        /// <param name="networkId">The ID of the current network.</param>
        /// <param name="userCount">The number of network users.</param>
        /// <param name="userInvitationCount">The number of network user invitations.</param>
        /// <param name="databaseCount">The number of network databases.</param>
        /// <param name="nodeCount">The number of network nodes.</param>
        /// <param name="edgeCount">The number of network edges.</param>
        /// <param name="nodeCollectionCount">The number of network node collections.</param>
        /// <returns>The navigation quick links for the network index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetNetworkNavigationQuickLinks(string networkId = null, int userCount = 0, int userInvitationCount = 0, int databaseCount = 0, int nodeCount = 0, int edgeCount = 0, int nodeCollectionCount = 0)
        {
            // Check if there is no network ID provided.
            if (string.IsNullOrEmpty(networkId))
            {
                // Assign the empty string to it.
                networkId = string.Empty;
            }
            // Get the corresponding navigation quick links.
            var networkUsersNavigationQuickLink = NetworkUsersNavigationQuickLink;
            var networkUserInvitationsNavigationQuickLink = NetworkUserInvitationsNavigationQuickLink;
            var networkDatabasesNavigationQuickLink = NetworkDatabasesNavigationQuickLink;
            var networkNodesNavigationQuickLink = NetworkNodesNavigationQuickLink;
            var networkEdgesNavigationQuickLink = NetworkEdgesNavigationQuickLink;
            var networkNodeCollectionsNavigationQuickLink = NetworkNodeCollectionsNavigationQuickLink;
            // Update the count and the route ID.
            networkUsersNavigationQuickLink.ItemCount = userCount;
            networkUsersNavigationQuickLink.RouteId = networkId;
            networkUserInvitationsNavigationQuickLink.ItemCount = userInvitationCount;
            networkUserInvitationsNavigationQuickLink.RouteId = networkId;
            networkDatabasesNavigationQuickLink.ItemCount = databaseCount;
            networkDatabasesNavigationQuickLink.RouteId = networkId;
            networkNodesNavigationQuickLink.ItemCount = nodeCount;
            networkNodesNavigationQuickLink.RouteId = networkId;
            networkEdgesNavigationQuickLink.ItemCount = edgeCount;
            networkEdgesNavigationQuickLink.RouteId = networkId;
            networkNodeCollectionsNavigationQuickLink.ItemCount = nodeCollectionCount;
            networkNodeCollectionsNavigationQuickLink.RouteId = networkId;
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                networkUsersNavigationQuickLink,
                networkUserInvitationsNavigationQuickLink,
                networkDatabasesNavigationQuickLink,
                networkNodesNavigationQuickLink,
                networkEdgesNavigationQuickLink,
                networkNodeCollectionsNavigationQuickLink
            };
        }

        /// <summary>
        /// Gets the navigation quick links for the analysis index page.
        /// </summary>
        /// <param name="analysisId">The ID of the current analysis.</param>
        /// <param name="userCount">The number of analysis users.</param>
        /// <param name="userInvitationCount">The number of analysis user invitations.</param>
        /// <param name="databaseCount">The number of analysis databases.</param>
        /// <param name="nodeCount">The number of analysis nodes.</param>
        /// <param name="edgeCount">The number of analysis edges.</param>
        /// <param name="nodeCollectionCount">The number of analysis node collections.</param>
        /// <returns>The navigation quick links for the analysis index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetAnalysisNavigationQuickLinks(string analysisId = null, int userCount = 0, int userInvitationCount = 0, int databaseCount = 0, int nodeCount = 0, int edgeCount = 0, int nodeCollectionCount = 0)
        {
            // Check if there is no analysis ID provided.
            if (string.IsNullOrEmpty(analysisId))
            {
                // Assign the empty string to it.
                analysisId = string.Empty;
            }
            // Get the corresponding navigation quick links.
            var analysisUsersNavigationQuickLink = AnalysisUsersNavigationQuickLink;
            var analysisUserInvitationsNavigationQuickLink = AnalysisUserInvitationsNavigationQuickLink;
            var analysisDatabasesNavigationQuickLink = AnalysisDatabasesNavigationQuickLink;
            var analysisNodesNavigationQuickLink = AnalysisNodesNavigationQuickLink;
            var analysisEdgesNavigationQuickLink = AnalysisEdgesNavigationQuickLink;
            var analysisNodeCollectionsNavigationQuickLink = AnalysisNodeCollectionsNavigationQuickLink;
            // Update the count and the route ID.
            analysisUsersNavigationQuickLink.ItemCount = userCount;
            analysisUsersNavigationQuickLink.RouteId = analysisId;
            analysisUserInvitationsNavigationQuickLink.ItemCount = userInvitationCount;
            analysisUserInvitationsNavigationQuickLink.RouteId = analysisId;
            analysisDatabasesNavigationQuickLink.ItemCount = databaseCount;
            analysisDatabasesNavigationQuickLink.RouteId = analysisId;
            analysisNodesNavigationQuickLink.ItemCount = nodeCount;
            analysisNodesNavigationQuickLink.RouteId = analysisId;
            analysisEdgesNavigationQuickLink.ItemCount = edgeCount;
            analysisEdgesNavigationQuickLink.RouteId = analysisId;
            analysisNodeCollectionsNavigationQuickLink.ItemCount = nodeCollectionCount;
            analysisNodeCollectionsNavigationQuickLink.RouteId = analysisId;
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                analysisUsersNavigationQuickLink,
                analysisUserInvitationsNavigationQuickLink,
                analysisDatabasesNavigationQuickLink,
                analysisNodesNavigationQuickLink,
                analysisEdgesNavigationQuickLink,
                analysisNodeCollectionsNavigationQuickLink
            };
        }
    }
}
