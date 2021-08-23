using NetControl4BioMed.Data.Models;

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
        /// Gets the navigation title card for the database details page.
        /// </summary>
        public static NavigationTitleCardViewModel DatabaseNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "Database",
            Icon = "fa-database"
        };

        /// <summary>
        /// Gets the navigation title card for the database protein field details page.
        /// </summary>
        public static NavigationTitleCardViewModel DatabaseProteinFieldNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "DatabaseProteinField",
            Icon = "fa-circle"
        };

        /// <summary>
        /// Gets the navigation title card for the database interaction field details page.
        /// </summary>
        public static NavigationTitleCardViewModel DatabaseInteractionFieldNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "DatabaseInteractionField",
            Icon = "fa-arrow-right"
        };

        /// <summary>
        /// Gets the navigation title card for the protein details page.
        /// </summary>
        public static NavigationTitleCardViewModel ProteinNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "Protein",
            Icon = "fa-circle"
        };

        /// <summary>
        /// Gets the navigation title card for the interaction details page.
        /// </summary>
        public static NavigationTitleCardViewModel InteractionNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "Interaction",
            Icon = "fa-arrow-right"
        };

        /// <summary>
        /// Gets the navigation title card for the protein collection details page.
        /// </summary>
        public static NavigationTitleCardViewModel ProteinCollectionNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "ProteinCollection",
            Icon = "fa-folder"
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
            Icon = "fa-code-branch"
        };

        /// <summary>
        /// Gets the navigation title card for the control path details page.
        /// </summary>
        public static NavigationTitleCardViewModel ControlPathNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "ControlPath",
            Icon = "fa-exchange-alt"
        };

        /// <summary>
        /// Gets the navigation title card for the path details page.
        /// </summary>
        public static NavigationTitleCardViewModel PathNavigationTitleCard { get; } = new NavigationTitleCardViewModel
        {
            Id = "Path",
            Icon = "fa-long-arrow-alt-right"
        };

        /// <summary>
        /// Gets the updated navigation title card for the user details page.
        /// </summary>
        /// <param name="user">The current user.</param>
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
        /// Gets the updated navigation title card for the database details page.
        /// </summary>
        /// <param name="database">The current database.</param>
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
        /// Gets the updated navigation title card for the database protein field details page.
        /// </summary>
        /// <param name="databaseProteinField">The current database protein field.</param>
        /// <returns>The navigation title card for the database protein field details page.</returns>
        public static NavigationTitleCardViewModel GetDatabaseProteinFieldNavigationTitleCard(DatabaseProteinField databaseProteinField)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = DatabaseProteinFieldNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = databaseProteinField.Name;
            navigationTitleCard.Subtitle = databaseProteinField.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the database interaction field details page.
        /// </summary>
        /// <param name="databaseInteractionField">The current database interaction field.</param>
        /// <returns>The navigation title card for the database interaction field details page.</returns>
        public static NavigationTitleCardViewModel GetDatabaseInteractionFieldNavigationTitleCard(DatabaseInteractionField databaseInteractionField)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = DatabaseInteractionFieldNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = databaseInteractionField.Name;
            navigationTitleCard.Subtitle = databaseInteractionField.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the protein details page.
        /// </summary>
        /// <param name="protein">The current protein.</param>
        /// <returns>The navigation title card for the protein details page.</returns>
        public static NavigationTitleCardViewModel GetProteinNavigationTitleCard(Protein protein)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = ProteinNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = protein.Name;
            navigationTitleCard.Subtitle = protein.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the interaction details page.
        /// </summary>
        /// <param name="interaction">The current interaction.</param>
        /// <returns>The navigation title card for the interaction details page.</returns>
        public static NavigationTitleCardViewModel GetInteractionNavigationTitleCard(Interaction interaction)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = InteractionNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = interaction.Name;
            navigationTitleCard.Subtitle = interaction.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the protein collection details page.
        /// </summary>
        /// <param name="proteinCollection">The current protein collection.</param>
        /// <returns>The navigation title card for the protein collection details page.</returns>
        public static NavigationTitleCardViewModel GetProteinCollectionNavigationTitleCard(ProteinCollection proteinCollection)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = ProteinCollectionNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = proteinCollection.Name;
            navigationTitleCard.Subtitle = proteinCollection.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }

        /// <summary>
        /// Gets the updated navigation title card for the network details page.
        /// </summary>
        /// <param name="network">The current network.</param>
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
        /// <param name="analysis">The current analysis.</param>
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
        /// <param name="controlPath">The current analysis.</param>
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

        /// <summary>
        /// Gets the updated navigation title card for the analysis details page.
        /// </summary>
        /// <param name="path">The current analysis.</param>
        /// <returns>The navigation title card for the analysis page.</returns>
        public static NavigationTitleCardViewModel GetPathNavigationTitleCard(Path path)
        {
            // Get the corresponding navigation title card.
            var navigationTitleCard = PathNavigationTitleCard;
            // Update the title and subtitle.
            navigationTitleCard.Title = "Path";
            navigationTitleCard.Subtitle = path.Id;
            // Return the navigation title card.
            return navigationTitleCard;
        }
    }
}
