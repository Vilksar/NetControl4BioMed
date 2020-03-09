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
            Title = "Roles",
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
            Color = "success",
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
            Color = "success",
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
            Link = "/Content/Created/Networks/Details/Nodes"
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
            Link = "/Content/Created/Networks/Details/Edges"
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
            Link = "/Content/Created/Networks/Details/Databases"
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
            Link = "/Content/Created/Networks/Details/NodeCollections"
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
            Link = "/Content/Created/Networks/Details/Users"
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
            Link = "/Content/Created/Networks/Details/Users"
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
            Link = "/Content/Created/Analyses/Details/Nodes"
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
            Link = "/Content/Created/Analyses/Details/Edges"
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
            Link = "/Content/Created/Analyses/Details/Databases"
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
            Link = "/Content/Created/Analyses/Details/NodeCollections"
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
            Link = "/Content/Created/Analyses/Details/Users"
        };

        /// <summary>
        /// Gets the user invitations navigation quick link for the analysis index page.
        /// </summary>
        public static NavigationQuickLinkViewModel AnalysisUserInvitationsNavigationQuickLink { get; } = new NavigationQuickLinkViewModel
        {
            Id = "UserInvitations",
            Title = "User invitations",
            Color = "primary",
            Icon = "fa-envelope-open",
            Width = 6,
            Link = "/Content/Created/Analyses/Details/Users"
        };

        /// <summary>
        /// Gets the navigation quick links for the administration index page.
        /// </summary>
        /// <param name="userCount">Represents the current number of users.</param>
        /// <param name="roleCount">Represents the current number of roles.</param>
        /// <param name="databaseCount">Represents the current number of databases.</param>
        /// <param name="nodeCollectionCount">Represents the current number of node collections.</param>
        /// <param name="nodeCount">Represents the current number of nodes.</param>
        /// <param name="edgeCount">Represents the current number of edges.</param>
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
        /// <param name="networkCount">Represents the current number of networks.</param>
        /// <param name="analysisCount">Represents the current number of analyses.</param>
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
        /// <param name="network">Represents the current network.</param>
        /// <returns>The navigation quick links for the network index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetNetworkNavigationQuickLinks(Network network)
        {
            // Get the corresponding navigation quick links.
            var networkNodesNavigationQuickLink = NetworkNodesNavigationQuickLink;
            var networkEdgesNavigationQuickLink = NetworkEdgesNavigationQuickLink;
            var networkDatabasesNavigationQuickLink = NetworkDatabasesNavigationQuickLink;
            var networkNodeCollectionsNavigationQuickLink = NetworkNodeCollectionsNavigationQuickLink;
            var networkUsersNavigationQuickLink = NetworkUsersNavigationQuickLink;
            var networkUserInvitationsNavigationQuickLink = NetworkUserInvitationsNavigationQuickLink;
            // Update the count and the route ID.
            networkNodesNavigationQuickLink.ItemCount = network.NetworkNodes.Count();
            networkNodesNavigationQuickLink.RouteId = network.Id;
            networkEdgesNavigationQuickLink.ItemCount = network.NetworkEdges.Count();
            networkEdgesNavigationQuickLink.RouteId = network.Id;
            networkDatabasesNavigationQuickLink.ItemCount = network.NetworkDatabases.Count();
            networkDatabasesNavigationQuickLink.RouteId = network.Id;
            networkNodeCollectionsNavigationQuickLink.ItemCount = network.NetworkNodeCollections.Count();
            networkNodeCollectionsNavigationQuickLink.RouteId = network.Id;
            networkUsersNavigationQuickLink.ItemCount = network.NetworkUsers.Count();
            networkUsersNavigationQuickLink.RouteId = network.Id;
            networkUserInvitationsNavigationQuickLink.ItemCount = network.NetworkUserInvitations.Count();
            networkUserInvitationsNavigationQuickLink.RouteId = network.Id;
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                networkNodesNavigationQuickLink,
                networkEdgesNavigationQuickLink,
                networkDatabasesNavigationQuickLink,
                networkNodeCollectionsNavigationQuickLink,
                networkUsersNavigationQuickLink,
                networkUserInvitationsNavigationQuickLink
            };
        }

        /// <summary>
        /// Gets the navigation quick links for the analysis index page.
        /// </summary>
        /// <param name="analysis">Represents the current analysis.</param>
        /// <returns>The navigation quick links for the analysis index page.</returns>
        public static IEnumerable<NavigationQuickLinkViewModel> GetAnalysisNavigationQuickLinks(Analysis analysis)
        {
            // Get the corresponding navigation quick links.
            var analysisNodesNavigationQuickLink = AnalysisNodesNavigationQuickLink;
            var analysisEdgesNavigationQuickLink = AnalysisEdgesNavigationQuickLink;
            var analysisDatabasesNavigationQuickLink = AnalysisDatabasesNavigationQuickLink;
            var analysisNodeCollectionsNavigationQuickLink = AnalysisNodeCollectionsNavigationQuickLink;
            var analysisUsersNavigationQuickLink = AnalysisUsersNavigationQuickLink;
            var analysisUserInvitationsNavigationQuickLink = AnalysisUserInvitationsNavigationQuickLink;
            // Update the count and the route ID.
            analysisNodesNavigationQuickLink.ItemCount = analysis.AnalysisNodes.Count();
            analysisNodesNavigationQuickLink.RouteId = analysis.Id;
            analysisEdgesNavigationQuickLink.ItemCount = analysis.AnalysisEdges.Count();
            analysisEdgesNavigationQuickLink.RouteId = analysis.Id;
            analysisDatabasesNavigationQuickLink.ItemCount = analysis.AnalysisDatabases.Count();
            analysisDatabasesNavigationQuickLink.RouteId = analysis.Id;
            analysisNodeCollectionsNavigationQuickLink.ItemCount = analysis.AnalysisNodeCollections.Count();
            analysisNodeCollectionsNavigationQuickLink.RouteId = analysis.Id;
            analysisUsersNavigationQuickLink.ItemCount = analysis.AnalysisUsers.Count();
            analysisUsersNavigationQuickLink.RouteId = analysis.Id;
            analysisUserInvitationsNavigationQuickLink.ItemCount = analysis.AnalysisUserInvitations.Count();
            analysisUserInvitationsNavigationQuickLink.RouteId = analysis.Id;
            // Return the navigation quick links.
            return new List<NavigationQuickLinkViewModel>
            {
                analysisNodesNavigationQuickLink,
                analysisEdgesNavigationQuickLink,
                analysisDatabasesNavigationQuickLink,
                analysisNodeCollectionsNavigationQuickLink,
                analysisUsersNavigationQuickLink,
                analysisUserInvitationsNavigationQuickLink
            };
        }
    }
}
