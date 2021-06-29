using NetControl4BioMed.Data.Models;
using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents a navigation area, formed by several navigation groups.
    /// </summary>
    public class NavigationAreaViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the area.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title (displayed text) of the area.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the area.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon of the area.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the color of the area.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the route ID of the area.
        /// </summary>
        public string RouteId { get; set; }

        /// <summary>
        /// Gets or sets the navigation groups in the area.
        /// </summary>
        public IEnumerable<NavigationGroupViewModel> NavigationGroups { get; set; }

        /// <summary>
        /// Gets the navigation area for the account pages.
        /// </summary>
        public static NavigationAreaViewModel AccountNavigationArea { get; } = new NavigationAreaViewModel
        {
            Id = "Account",
            Title = "Account",
            Description = string.Empty,
            Icon = "fa-user-circle",
            Color = "info",
            NavigationGroups = new List<NavigationGroupViewModel>
            {
                new NavigationGroupViewModel
                {
                    Id = "Index",
                    Title = "Overview",
                    Description = string.Empty,
                    Icon = "fa-chart-bar",
                    Color = "info",
                    Link = "/Account/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Manage",
                    Title = "Manage",
                    Description = string.Empty,
                    Icon = "fa-cog",
                    Color = "info",
                    Link = "/Account/Manage/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Profile",
                            Title = "Profile",
                            Description = string.Empty,
                            Icon = "fa-user-edit",
                            Color = "light",
                            Link = "/Account/Manage/Profile/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Password",
                            Title = "Password",
                            Description = string.Empty,
                            Icon = "fa-user-lock",
                            Color = "light",
                            Link = "/Account/Manage/Password/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "ExternalLogins",
                            Title = "External logins",
                            Description = string.Empty,
                            Icon = "fa-user-cog",
                            Color = "light",
                            Link = "/Account/Manage/ExternalLogins/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "TwoFactorAuthentication",
                            Title = "Two-factor authentication",
                            Description = string.Empty,
                            Icon = "fa-user-shield",
                            Color = "light",
                            Link = "/Account/Manage/TwoFactorAuthentication/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "PersonalData",
                            Title = "Personal data",
                            Description = string.Empty,
                            Icon = "fa-user-check",
                            Color = "light",
                            Link = "/Account/Manage/PersonalData/Index"
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Gets the navigation area for the administration pages.
        /// </summary>
        public static NavigationAreaViewModel AdministrationNavigationArea { get; } = new NavigationAreaViewModel
        {
            Id = "Administration",
            Title = "Administration",
            Description = string.Empty,
            Icon = "fa-toolbox",
            Color = "primary",
            NavigationGroups = new List<NavigationGroupViewModel>
            {
                new NavigationGroupViewModel
                {
                    Id = "Index",
                    Title = "Overview",
                    Description = string.Empty,
                    Icon = "fa-chart-bar",
                    Color = "primary",
                    Link = "/Administration/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Accounts",
                    Title = "Accounts",
                    Description = string.Empty,
                    Icon = "fa-users",
                    Color = "primary",
                    Link = "/Administration/Accounts/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Users",
                            Title = "Users",
                            Description = string.Empty,
                            Icon = "fa-user",
                            Color = "light",
                            Link = "/Administration/Accounts/Users/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Roles",
                            Title = "Roles",
                            Description = string.Empty,
                            Icon = "fa-tag",
                            Color = "light",
                            Link = "/Administration/Accounts/Roles/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "UserRoles",
                            Title = "User roles",
                            Description = string.Empty,
                            Icon = "fa-user-tag",
                            Color = "light",
                            Link = "/Administration/Accounts/UserRoles/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Permissions",
                    Title = "Permissions",
                    Description = string.Empty,
                    Icon = "fa-lock",
                    Color = "primary",
                    Link = "/Administration/Permissions/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseUsers",
                            Title = "Database users",
                            Description = string.Empty,
                            Icon = "fa-database",
                            Color = "light",
                            Link = "/Administration/Permissions/DatabaseUsers/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Databases",
                    Title = "Databases",
                    Description = string.Empty,
                    Icon = "fa-database",
                    Color = "primary",
                    Link = "/Administration/Databases/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Databases",
                            Title = "Databases",
                            Description = string.Empty,
                            Icon = "fa-database",
                            Color = "light",
                            Link = "/Administration/Databases/Databases/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseProteinFields",
                            Title = "Database protein fields",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Administration/Databases/DatabaseProteinFields/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseInteractionFields",
                            Title = "Database interaction fields",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Administration/Databases/DatabaseInteractionFields/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Data",
                    Title = "Data",
                    Description = string.Empty,
                    Icon = "fa-table",
                    Color = "primary",
                    Link = "/Administration/Data/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Proteins",
                            Title = "Proteins",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Administration/Data/Proteins/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Interactions",
                            Title = "Interactions",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Administration/Data/Interactions/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "ProteinCollections",
                            Title = "Collections",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/Administration/Data/ProteinCollections/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Relationships",
                    Title = "Relationships",
                    Description = string.Empty,
                    Icon = "fa-heart",
                    Color = "primary",
                    Link = "/Administration/Relationships/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseProteins",
                            Title = "Database proteins",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Administration/Relationships/DatabaseProteins/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseProteinFieldProteins",
                            Title = "Database protein field proteins",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Administration/Relationships/DatabaseProteinFieldProteins/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseInteractions",
                            Title = "Database interactions",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Administration/Relationships/DatabaseInteractions/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseInteractionFieldInteractions",
                            Title = "Database interaction field interactions",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Administration/Relationships/DatabaseInteractionFieldInteractions/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "InteractionProteins",
                            Title = "Interaction proteins",
                            Description = string.Empty,
                            Icon = "fa-arrow-circle-right",
                            Color = "light",
                            Link = "/Administration/Relationships/InteractionProteins/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "ProteinCollectionTypes",
                            Title = "Protein collection types",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/Administration/Relationships/ProteinCollectionTypes/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "ProteinCollectionProteins",
                            Title = "Protein collection proteins",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/Administration/Relationships/ProteinCollectionProteins/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Created",
                    Title = "Created",
                    Description = string.Empty,
                    Icon = "fa-hammer",
                    Color = "primary",
                    Link = "/Administration/Created/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Networks",
                            Title = "Networks",
                            Description = string.Empty,
                            Icon = "fa-share-alt",
                            Color = "light",
                            Link = "/Administration/Created/Networks/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Analyses",
                            Title = "Analyses",
                            Description = string.Empty,
                            Icon = "fa-desktop",
                            Color = "light",
                            Link = "/Administration/Created/Analyses/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Other",
                    Title = "Other",
                    Description = string.Empty,
                    Icon = "fa-code",
                    Color = "primary",
                    Link = "/Administration/Other/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "BackgroundTasks",
                            Title = "Background tasks",
                            Description = string.Empty,
                            Icon = "fa-tasks",
                            Color = "light",
                            Link = "/Administration/Other/BackgroundTasks/Index"
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Gets the navigation area for the public data pages.
        /// </summary>
        public static NavigationAreaViewModel PublicDataNavigationArea { get; } = new NavigationAreaViewModel
        {
            Id = "PublicData",
            Title = "Public networks",
            Description = string.Empty,
            Icon = "fa-users",
            Color = "success",
            NavigationGroups = new List<NavigationGroupViewModel>
            {
                new NavigationGroupViewModel
                {
                    Id = "Index",
                    Title = "Overview",
                    Description = string.Empty,
                    Icon = "fa-chart-bar",
                    Color = "success",
                    Link = "/PublicData/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Networks",
                    Title = "Public networks",
                    Description = string.Empty,
                    Icon = "fa-share-alt",
                    Color = "success",
                    Link = "/PublicData/Networks/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Analyses",
                    Title = "Public analyses",
                    Description = string.Empty,
                    Icon = "fa-desktop",
                    Color = "success",
                    Link = "/PublicData/Analyses/Index"
                }
            }
        };

        /// <summary>
        /// Gets the navigation area for the private data pages.
        /// </summary>
        public static NavigationAreaViewModel PrivateDataNavigationArea { get; } = new NavigationAreaViewModel
        {
            Id = "PrivateData",
            Title = "My networks",
            Description = string.Empty,
            Icon = "fa-user",
            Color = "success",
            NavigationGroups = new List<NavigationGroupViewModel>
            {
                new NavigationGroupViewModel
                {
                    Id = "Index",
                    Title = "Overview",
                    Description = string.Empty,
                    Icon = "fa-chart-bar",
                    Color = "success",
                    Link = "/PrivateData/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Networks",
                    Title = "My networks",
                    Description = string.Empty,
                    Icon = "fa-share-alt",
                    Color = "success",
                    Link = "/PrivateData/Networks/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Analyses",
                    Title = "My analyses",
                    Description = string.Empty,
                    Icon = "fa-desktop",
                    Color = "success",
                    Link = "/PrivateData/Analyses/Index"
                }
            }
        };

        /// <summary>
        /// Gets the navigation area for the available data pages.
        /// </summary>
        public static NavigationAreaViewModel AvailableDataNavigationArea { get; } = new NavigationAreaViewModel
        {
            Id = "AvailableData",
            Title = "AvailableData",
            Description = string.Empty,
            Icon = "fa-database",
            Color = "primary",
            NavigationGroups = new List<NavigationGroupViewModel>
            {
                new NavigationGroupViewModel
                {
                    Id = "Index",
                    Title = "Overview",
                    Description = string.Empty,
                    Icon = "fa-chart-bar",
                    Color = "primary",
                    Link = "/AvailableData/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Databases",
                    Title = "Available databases",
                    Description = string.Empty,
                    Icon = "fa-database",
                    Color = "primary",
                    Link = "/AvailableData/Databases/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Databases",
                            Title = "Databases",
                            Description = string.Empty,
                            Icon = "fa-database",
                            Color = "light",
                            Link = "/AvailableData/Databases/Databases/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseProteinFields",
                            Title = "Protein data",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/AvailableData/Databases/DatabaseProteinFields/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseInteractionFields",
                            Title = "Interaction data",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/AvailableData/Databases/DatabaseInteractionFields/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Data",
                    Title = "Browse data",
                    Description = string.Empty,
                    Icon = "fa-table",
                    Color = "primary",
                    Link = "/AvailableData/Data/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Proteins",
                            Title = "Proteins",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/AvailableData/Data/Proteins/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Interactions",
                            Title = "Interactions",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/AvailableData/Data/Interactions/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "ProteinCollections",
                            Title = "Protein collections",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/AvailableData/Data/ProteinCollections/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Created",
                    Title = "Created",
                    Description = string.Empty,
                    Icon = "fa-hammer",
                    Color = "primary",
                    Link = "/AvailableData/Created/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Networks",
                            Title = "Networks",
                            Description = string.Empty,
                            Icon = "fa-share-alt",
                            Color = "light",
                            Link = "/AvailableData/Created/Networks/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Analyses",
                            Title = "Analyses",
                            Description = string.Empty,
                            Icon = "fa-desktop",
                            Color = "light",
                            Link = "/AvailableData/Created/Analyses/Index"
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Gets the navigation area for the available data network pages.
        /// </summary>
        public static NavigationAreaViewModel AvailableDataNetworkNavigationArea { get; } = new NavigationAreaViewModel
        {
            Id = "Network",
            Title = "Network",
            Description = string.Empty,
            Icon = "fa-share-alt",
            Color = "dark",
            NavigationGroups = new List<NavigationGroupViewModel>
            {
                new NavigationGroupViewModel
                {
                    Id = "Index",
                    Title = "Overview",
                    Description = string.Empty,
                    Icon = "fa-chart-bar",
                    Color = "dark",
                    Link = "/AvailableData/Created/Networks/Details/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Accounts",
                    Title = "Accounts",
                    Description = string.Empty,
                    Icon = "fa-users",
                    Color = "dark",
                    Link = "/AvailableData/Created/Networks/Details/Accounts/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Users",
                            Title = "Users",
                            Description = string.Empty,
                            Icon = "fa-user",
                            Color = "light",
                            Link = "/AvailableData/Created/Networks/Details/Accounts/Users/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Databases",
                    Title = "Databases",
                    Description = string.Empty,
                    Icon = "fa-database",
                    Color = "dark",
                    Link = "/AvailableData/Created/Networks/Details/Databases/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Databases",
                            Title = "Databases",
                            Description = string.Empty,
                            Icon = "fa-database",
                            Color = "light",
                            Link = "/AvailableData/Created/Networks/Details/Databases/Databases/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Data",
                    Title = "Data",
                    Description = string.Empty,
                    Icon = "fa-table",
                    Color = "dark",
                    Link = "/AvailableData/Created/Networks/Details/Data/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Proteins",
                            Title = "Proteins",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/AvailableData/Created/Networks/Details/Data/Proteins/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Interactions",
                            Title = "Interactions",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/AvailableData/Created/Networks/Details/Data/Interactions/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "ProteinCollections",
                            Title = "Collections",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/AvailableData/Created/Networks/Details/Data/ProteinCollections/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Created",
                    Title = "Created",
                    Description = string.Empty,
                    Icon = "fa-hammer",
                    Color = "dark",
                    Link = "/AvailableData/Created/Networks/Details/Created/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Analyses",
                            Title = "Analyses",
                            Description = string.Empty,
                            Icon = "fa-desktop",
                            Color = "light",
                            Link = "/AvailableData/Created/Networks/Details/Created/Analyses/Index"
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Gets the navigation area for the available data analysis pages.
        /// </summary>
        public static NavigationAreaViewModel AvailableDataAnalysisNavigationArea { get; } = new NavigationAreaViewModel
        {
            Id = "Analysis",
            Title = "Analysis",
            Description = string.Empty,
            Icon = "fa-desktop",
            Color = "dark",
            NavigationGroups = new List<NavigationGroupViewModel>
            {
                new NavigationGroupViewModel
                {
                    Id = "Index",
                    Title = "Overview",
                    Description = string.Empty,
                    Icon = "fa-chart-bar",
                    Color = "dark",
                    Link = "/AvailableData/Created/Analyses/Details/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Accounts",
                    Title = "Accounts",
                    Description = string.Empty,
                    Icon = "fa-users",
                    Color = "dark",
                    Link = "/AvailableData/Created/Analyses/Details/Accounts/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Users",
                            Title = "Users",
                            Description = string.Empty,
                            Icon = "fa-user",
                            Color = "light",
                            Link = "/AvailableData/Created/Analyses/Details/Accounts/Users/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Databases",
                    Title = "Databases",
                    Description = string.Empty,
                    Icon = "fa-database",
                    Color = "dark",
                    Link = "/AvailableData/Created/Analyses/Details/Databases/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Databases",
                            Title = "Databases",
                            Description = string.Empty,
                            Icon = "fa-database",
                            Color = "light",
                            Link = "/AvailableData/Created/Analyses/Details/Databases/Databases/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Data",
                    Title = "Data",
                    Description = string.Empty,
                    Icon = "fa-table",
                    Color = "dark",
                    Link = "/AvailableData/Created/Analyses/Details/Data/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Proteins",
                            Title = "Proteins",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/AvailableData/Created/Analyses/Details/Data/Proteins/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Interactions",
                            Title = "Interactions",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/AvailableData/Created/Analyses/Details/Data/Interactions/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "ProteinCollections",
                            Title = "Collections",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/AvailableData/Analyses/Details/Data/ProteinCollections/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Results",
                    Title = "Results",
                    Description = string.Empty,
                    Icon = "fa-hammer",
                    Color = "dark",
                    Link = "/AvailableData/Created/Analyses/Details/Results/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "ControlPaths",
                            Title = "Control paths",
                            Description = string.Empty,
                            Icon = "fa-exchange-alt",
                            Color = "light",
                            Link = "/AvailableData/Created/Analyses/Details/Results/ControlPaths/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Paths",
                            Title = "Paths",
                            Description = string.Empty,
                            Icon = "fa-long-arrow-alt-right",
                            Color = "light",
                            Link = "/AvailableData/Created/Analyses/Details/Results/Paths/Index"
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Gets the updated navigation area for the available data network pages.
        /// </summary>
        /// <param name="network">Represents the current network.</param>
        /// <returns>The navigation area for the network pages.</returns>
        public static NavigationAreaViewModel GetAvailableDataNetworkNavigationArea(Network network)
        {
            // Get the corresponding navigation area.
            var navigationArea = AvailableDataNetworkNavigationArea;
            // Update the route ID.
            navigationArea.RouteId = network.Id;
            // Return the navigation area.
            return navigationArea;
        }

        /// <summary>
        /// Gets the updated navigation area for the content PPI analysis pages.
        /// </summary>
        /// <param name="analysis">Represents the current analysis.</param>
        /// <returns>The navigation area for the analysis pages.</returns>
        public static NavigationAreaViewModel GetAvailableDataAnalysisNavigationArea(Analysis analysis)
        {
            // Get the corresponding navigation area.
            var navigationArea = AvailableDataAnalysisNavigationArea;
            // Update the route ID.
            navigationArea.RouteId = analysis.Id;
            // Return the navigation area.
            return navigationArea;
        }
    }
}
