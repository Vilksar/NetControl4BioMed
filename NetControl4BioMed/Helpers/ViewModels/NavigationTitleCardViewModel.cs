using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents a navigation title card.
    /// </summary>
    public class NavigationTitleCardViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the title card.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title (displayed text) of the title card.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the subtitle (displayed text) of the title card.
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets the icon of the banner.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets the navigation title card for the user details page.
        /// </summary>
        public static NavigationTitleCardViewModel UserNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "User",
            Icon = "fa-user"
        };

        /// <summary>
        /// Gets the navigation title card for the database type details page.
        /// </summary>
        public static NavigationTitleCardViewModel DatabaseTypeNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "DatabaseType",
            Icon = "fa-font"
        };

        /// <summary>
        /// Gets the navigation title card for the database details page.
        /// </summary>
        public static NavigationTitleCardViewModel DatabaseNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "Database",
            Icon = "fa-database"
        };

        /// <summary>
        /// Gets the navigation title card for the database node field details page.
        /// </summary>
        public static NavigationTitleCardViewModel DatabaseNodeFieldNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "DatabaseNodeField",
            Icon = "fa-circle"
        };

        /// <summary>
        /// Gets the navigation title card for the database edge field details page.
        /// </summary>
        public static NavigationTitleCardViewModel DatabaseEdgeFieldNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "DatabaseEdgeField",
            Icon = "fa-arrow-right"
        };

        /// <summary>
        /// Gets the navigation title card for the edge details page.
        /// </summary>
        public static NavigationTitleCardViewModel EdgeNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "Edge",
            Icon = "fa-arrow-right"
        };

        /// <summary>
        /// Gets the navigation title card for the node details page.
        /// </summary>
        public static NavigationTitleCardViewModel NodeNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "Node",
            Icon = "fa-circle"
        };

        /// <summary>
        /// Gets the navigation title card for the node collection details page.
        /// </summary>
        public static NavigationTitleCardViewModel NodeCollectionNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "NodeCollection",
            Icon = "fa-folder"
        };

        /// <summary>
        /// Gets the navigation title card for the sample details page.
        /// </summary>
        public static NavigationTitleCardViewModel SampleNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "Sample",
            Icon = "fa-paste"
        };

        /// <summary>
        /// Gets the navigation title card for the network details page.
        /// </summary>
        public static NavigationTitleCardViewModel NetworkNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "Network",
            Icon = "fa-share-alt"
        };

        /// <summary>
        /// Gets the navigation title card for the analysis details page.
        /// </summary>
        public static NavigationTitleCardViewModel AnalysisNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "Analysis",
            Icon = "fa-desktop"
        };

        /// <summary>
        /// Gets the navigation title card for the control path details page.
        /// </summary>
        public static NavigationTitleCardViewModel ControlPathNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "ControlPath",
            Icon = "fa-gamepad"
        };

        /// <summary>
        /// Gets the updated navigation title card for the user details page.
        /// </summary>
        /// <param name="user">Represents the current user.</param>
        /// <returns>The navigation title card for the user details page.</returns>
        public static NavigationTitleCardViewModel GetUserNavigationTitleCard(User user)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = UserNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = user.Email;
            navigationTitleCard.Subtitle = user.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the database type details page.
        /// </summary>
        /// <param name="databaseType">Represents the current database type.</param>
        /// <returns>The navigation title card for the database type pages.</returns>
        public static NavigationTitleCardViewModel GetDatabaseTypeNavigationTitleCard(DatabaseType databaseType)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = DatabaseTypeNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = databaseType.Name;
            navigationTitleCard.Subtitle = databaseType.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the database details page.
        /// </summary>
        /// <param name="database">Represents the current database.</param>
        /// <returns>The navigation title card for the database details page.</returns>
        public static NavigationTitleCardViewModel GetDatabaseNavigationTitleCard(Database database)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = DatabaseNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = database.Name;
            navigationTitleCard.Subtitle = database.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the database node field details page.
        /// </summary>
        /// <param name="databaseNodeField">Represents the current database node field.</param>
        /// <returns>The navigation title card for the database node field details page.</returns>
        public static NavigationTitleCardViewModel GetDatabaseNodeFieldNavigationTitleCard(DatabaseNodeField databaseNodeField)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = DatabaseNodeFieldNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = databaseNodeField.Name;
            navigationTitleCard.Subtitle = databaseNodeField.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the database edge field details page.
        /// </summary>
        /// <param name="databaseEdgeField">Represents the current database edge field.</param>
        /// <returns>The navigation title card for the database edge field details page.</returns>
        public static NavigationTitleCardViewModel GetDatabaseEdgeFieldNavigationTitleCard(DatabaseEdgeField databaseEdgeField)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = DatabaseEdgeFieldNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = databaseEdgeField.Name;
            navigationTitleCard.Subtitle = databaseEdgeField.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the node details page.
        /// </summary>
        /// <param name="node">Represents the current node.</param>
        /// <returns>The navigation title card for the node details page.</returns>
        public static NavigationTitleCardViewModel GetNodeNavigationTitleCard(Node node)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = NodeNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = node.Name;
            navigationTitleCard.Subtitle = node.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the edge details page.
        /// </summary>
        /// <param name="edge">Represents the current edge.</param>
        /// <returns>The navigation title card for the edge details page.</returns>
        public static NavigationTitleCardViewModel GetEdgeNavigationTitleCard(Edge edge)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = EdgeNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = edge.Name;
            navigationTitleCard.Subtitle = edge.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the node collection details page.
        /// </summary>
        /// <param name="nodeCollection">Represents the current node collection.</param>
        /// <returns>The navigation title card for the node collection details page.</returns>
        public static NavigationTitleCardViewModel GetNodeCollectionNavigationTitleCard(NodeCollection nodeCollection)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = NodeCollectionNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = nodeCollection.Name;
            navigationTitleCard.Subtitle = nodeCollection.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the sample details page.
        /// </summary>
        /// <param name="nodeCollection">Represents the current sample.</param>
        /// <returns>The navigation title card for the sample details page.</returns>
        public static NavigationTitleCardViewModel GetSampleNavigationTitleCard(Sample sample)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = SampleNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = sample.Name;
            navigationTitleCard.Subtitle = sample.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the network details page.
        /// </summary>
        /// <param name="network">Represents the current network.</param>
        /// <returns>The navigation title card for the network page.</returns>
        public static NavigationTitleCardViewModel GetNetworkNavigationTitleCard(Network network)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = NetworkNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = network.Name;
            navigationTitleCard.Subtitle = network.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the analysis details page.
        /// </summary>
        /// <param name="analysis">Represents the current analysis.</param>
        /// <returns>The navigation title card for the analysis page.</returns>
        public static NavigationTitleCardViewModel GetAnalysisNavigationTitleCard(Analysis analysis)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = AnalysisNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = analysis.Name;
            navigationTitleCard.Subtitle = analysis.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the analysis details page.
        /// </summary>
        /// <param name="analysis">Represents the current analysis.</param>
        /// <returns>The navigation title card for the analysis page.</returns>
        public static NavigationTitleCardViewModel GetControlPathNavigationTitleCard(ControlPath controlPath)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = ControlPathNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = "Control path";
            navigationTitleCard.Subtitle = controlPath.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }
    }
}
