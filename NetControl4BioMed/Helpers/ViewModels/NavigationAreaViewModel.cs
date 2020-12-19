using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            Icon = "fa-user",
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
            Icon = "fa-cog",
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
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseUserInvitations",
                            Title = "Database user invitations",
                            Description = string.Empty,
                            Icon = "fa-envelope-open",
                            Color = "light",
                            Link = "/Administration/Permissions/DatabaseUserInvitations/Index"
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
                            Id = "DatabaseTypes",
                            Title = "Database types",
                            Description = string.Empty,
                            Icon = "fa-font",
                            Color = "light",
                            Link = "/Administration/Databases/DatabaseTypes/Index"
                        },
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
                            Id = "DatabaseNodeFields",
                            Title = "Database node fields",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Administration/Databases/DatabaseNodeFields/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseEdgeFields",
                            Title = "Database edge fields",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Administration/Databases/DatabaseEdgeFields/Index"
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
                            Id = "Nodes",
                            Title = "Nodes",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Administration/Data/Nodes/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Edges",
                            Title = "Edges",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Administration/Data/Edges/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "NodeCollections",
                            Title = "Node collections",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/Administration/Data/NodeCollections/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Samples",
                            Title = "Samples",
                            Description = string.Empty,
                            Icon = "fa-paste",
                            Color = "light",
                            Link = "/Administration/Data/Samples/Index"
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
                            Id = "DatabaseNodes",
                            Title = "Database nodes",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Administration/Relationships/DatabaseNodes/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseNodeFieldNodes",
                            Title = "Database node field nodes",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Administration/Relationships/DatabaseNodeFieldNodes/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseEdges",
                            Title = "Database edges",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Administration/Relationships/DatabaseEdges/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseEdgeFieldEdges",
                            Title = "Database edge field edges",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Administration/Relationships/DatabaseEdgeFieldEdges/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "EdgeNodes",
                            Title = "Edge nodes",
                            Description = string.Empty,
                            Icon = "fa-arrow-circle-right",
                            Color = "light",
                            Link = "/Administration/Relationships/EdgeNodes/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "NodeCollectionTypes",
                            Title = "Node collection types",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/Administration/Relationships/NodeCollectionTypes/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "NodeCollectionDatabases",
                            Title = "Node collection databases",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/Administration/Relationships/NodeCollectionDatabases/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "NodeCollectionNodes",
                            Title = "Node collection nodes",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/Administration/Relationships/NodeCollectionNodes/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "SampleDatabases",
                            Title = "Sample databases",
                            Description = string.Empty,
                            Icon = "fa-paste",
                            Color = "light",
                            Link = "/Administration/Relationships/SampleDatabases/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Created",
                    Title = "Created",
                    Description = string.Empty,
                    Icon = "fa-smile",
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
                    Icon = "fa-ellipsis-h",
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
        /// Gets the navigation area for the content pages.
        /// </summary>
        public static NavigationAreaViewModel ContentNavigationArea { get; } = new NavigationAreaViewModel
        {
            Id = "Content",
            Title = "Content",
            Description = string.Empty,
            Icon = "fa-box-open",
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
                    Link = "/Content/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "DatabaseTypes",
                    Title = "Types",
                    Description = string.Empty,
                    Icon = "fa-list",
                    Color = "success",
                    Link = "/Content/DatabaseTypes/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Generic",
                            Title = "Generic",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/Generic/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "PPI",
                            Title = "PPI",
                            Description = string.Empty,
                            Icon = "fa-dna",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Index"
                        },
                    }
                }
            }
        };

        /// <summary>
        /// Gets the navigation area for the content generic pages.
        /// </summary>
        public static NavigationAreaViewModel ContentGenericNavigationArea { get; } = new NavigationAreaViewModel
        {
            Id = "Generic",
            Title = "Generic",
            Description = string.Empty,
            Icon = "fa-circle",
            Color = "warning",
            NavigationGroups = new List<NavigationGroupViewModel>
            {
                new NavigationGroupViewModel
                {
                    Id = "Index",
                    Title = "Overview",
                    Description = string.Empty,
                    Icon = "fa-chart-bar",
                    Color = "warning",
                    Link = "/Content/DatabaseTypes/Generic/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Created",
                    Title = "My data",
                    Description = string.Empty,
                    Icon = "fa-user",
                    Color = "warning",
                    Link = "/Content/DatabaseTypes/Generic/Created/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Networks",
                            Title = "Networks",
                            Description = string.Empty,
                            Icon = "fa-share-alt",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/Generic/Created/Networks/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Analyses",
                            Title = "Analyses",
                            Description = string.Empty,
                            Icon = "fa-desktop",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/Generic/Created/Analyses/Index"
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Gets the navigation area for the generic network pages.
        /// </summary>
        public static NavigationAreaViewModel ContentGenericNetworkNavigationArea { get; } = new NavigationAreaViewModel
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
                    Link = "/Content/DatabaseTypes/Generic/Created/Networks/Details/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Accounts",
                    Title = "Accounts",
                    Description = string.Empty,
                    Icon = "fa-user-friends",
                    Color = "dark",
                    Link = "/Content/DatabaseTypes/Generic/Created/Networks/Details/Accounts/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Users",
                            Title = "Users",
                            Description = string.Empty,
                            Icon = "fa-user-friends",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/Generic/Created/Networks/Details/Accounts/Users/Index"
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
                    Link = "/Content/DatabaseTypes/Generic/Created/Networks/Details/Data/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Nodes",
                            Title = "Nodes",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/Generic/Created/Networks/Details/Data/Nodes/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Edges",
                            Title = "Edges",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/Generic/Created/Networks/Details/Data/Edges/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Created",
                    Title = "My data",
                    Description = string.Empty,
                    Icon = "fa-user",
                    Color = "dark",
                    Link = "/Content/DatabaseTypes/Generic/Created/Networks/Details/Created/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Analyses",
                            Title = "Analyses",
                            Description = string.Empty,
                            Icon = "fa-desktop",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/Generic/Created/Networks/Details/Created/Analyses/Index"
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Gets the navigation area for the generic analysis pages.
        /// </summary>
        public static NavigationAreaViewModel ContentGenericAnalysisNavigationArea { get; } = new NavigationAreaViewModel
        {
            Id = "Analysis",
            Title = "Analysis",
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
                    Link = "/Content/DatabaseTypes/Generic/Created/Analyses/Details/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Accounts",
                    Title = "Accounts",
                    Description = string.Empty,
                    Icon = "fa-user-friends",
                    Color = "dark",
                    Link = "/Content/DatabaseTypes/Generic/Created/Analyses/Details/Accounts/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Users",
                            Title = "Users",
                            Description = string.Empty,
                            Icon = "fa-user-friends",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/Generic/Created/Analyses/Details/Accounts/Users/Index"
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
                    Link = "/Content/DatabaseTypes/Generic/Created/Analyses/Details/Data/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Nodes",
                            Title = "Nodes",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/Generic/Created/Analyses/Details/Data/Nodes/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Edges",
                            Title = "Edges",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/Generic/Created/Analyses/Details/Data/Edges/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Created",
                    Title = "My data",
                    Description = string.Empty,
                    Icon = "fa-user",
                    Color = "dark",
                    Link = "/Content/DatabaseTypes/Generic/Created/Analyses/Details/Created/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Networks",
                            Title = "Networks",
                            Description = string.Empty,
                            Icon = "fa-desktop",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/Generic/Created/Analyses/Details/Created/Networks/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "ControlPaths",
                            Title = "Control paths",
                            Description = string.Empty,
                            Icon = "fa-gamepad",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/Generic/Created/Analyses/Details/Created/ControlPaths/Index"
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Gets the navigation area for the content PPI pages.
        /// </summary>
        public static NavigationAreaViewModel ContentPPINavigationArea { get; } = new NavigationAreaViewModel
        {
            Id = "PPI",
            Title = "PPI",
            Description = string.Empty,
            Icon = "fa-dna",
            Color = "warning",
            NavigationGroups = new List<NavigationGroupViewModel>
            {
                new NavigationGroupViewModel
                {
                    Id = "Index",
                    Title = "Overview",
                    Description = string.Empty,
                    Icon = "fa-chart-bar",
                    Color = "warning",
                    Link = "/Content/DatabaseTypes/PPI/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Databases",
                    Title = "Available databases",
                    Description = string.Empty,
                    Icon = "fa-database",
                    Color = "warning",
                    Link = "/Content/DatabaseTypes/PPI/Databases/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Databases",
                            Title = "Databases",
                            Description = string.Empty,
                            Icon = "fa-database",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Databases/Databases/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseNodeFields",
                            Title = "Protein data",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Databases/DatabaseNodeFields/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "DatabaseEdgeFields",
                            Title = "Interaction data",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Databases/DatabaseEdgeFields/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Data",
                    Title = "Available data",
                    Description = string.Empty,
                    Icon = "fa-table",
                    Color = "warning",
                    Link = "/Content/DatabaseTypes/PPI/Data/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Nodes",
                            Title = "Protein data",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Data/Nodes/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Edges",
                            Title = "Interaction data",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Data/Edges/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "SourceNodeCollections",
                            Title = "Drug-target data",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Data/SourceNodeCollections/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "TargetNodeCollections",
                            Title = "Cell-line data",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Data/TargetNodeCollections/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Created",
                    Title = "My data",
                    Description = string.Empty,
                    Icon = "fa-user",
                    Color = "warning",
                    Link = "/Content/DatabaseTypes/PPI/Created/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Networks",
                            Title = "Networks",
                            Description = string.Empty,
                            Icon = "fa-share-alt",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Networks/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Analyses",
                            Title = "Analyses",
                            Description = string.Empty,
                            Icon = "fa-desktop",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Index"
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Gets the navigation area for the PPI network pages.
        /// </summary>
        public static NavigationAreaViewModel ContentPPINetworkNavigationArea { get; } = new NavigationAreaViewModel
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
                    Link = "/Content/DatabaseTypes/PPI/Created/Networks/Details/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Accounts",
                    Title = "Accounts",
                    Description = string.Empty,
                    Icon = "fa-user-friends",
                    Color = "dark",
                    Link = "/Content/DatabaseTypes/PPI/Created/Networks/Details/Accounts/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Users",
                            Title = "Users",
                            Description = string.Empty,
                            Icon = "fa-user-friends",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Networks/Details/Accounts/Users/Index"
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
                    Link = "/Content/DatabaseTypes/PPI/Created/Networks/Details/Databases/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Databases",
                            Title = "Databases",
                            Description = string.Empty,
                            Icon = "fa-database",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Networks/Details/Databases/Databases/Index"
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
                    Link = "/Content/DatabaseTypes/PPI/Created/Networks/Details/Data/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Nodes",
                            Title = "Proteins",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Networks/Details/Data/Nodes/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Edges",
                            Title = "Interactions",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Networks/Details/Data/Edges/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "NodeCollections",
                            Title = "Protein collections",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Networks/Details/Data/NodeCollections/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Created",
                    Title = "My data",
                    Description = string.Empty,
                    Icon = "fa-user",
                    Color = "dark",
                    Link = "/Content/DatabaseTypes/PPI/Created/Networks/Details/Created/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Analyses",
                            Title = "Analyses",
                            Description = string.Empty,
                            Icon = "fa-desktop",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Networks/Details/Created/Analyses/Index"
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Gets the navigation area for the PPI analysis pages.
        /// </summary>
        public static NavigationAreaViewModel ContentPPIAnalysisNavigationArea { get; } = new NavigationAreaViewModel
        {
            Id = "Analysis",
            Title = "Analysis",
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
                    Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Index"
                },
                new NavigationGroupViewModel
                {
                    Id = "Accounts",
                    Title = "Accounts",
                    Description = string.Empty,
                    Icon = "fa-user-friends",
                    Color = "dark",
                    Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Accounts/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Users",
                            Title = "Users",
                            Description = string.Empty,
                            Icon = "fa-user-friends",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Accounts/Users/Index"
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
                    Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Databases/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Databases",
                            Title = "Databases",
                            Description = string.Empty,
                            Icon = "fa-database",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Databases/Databases/Index"
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
                    Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Data/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Nodes",
                            Title = "Proteins",
                            Description = string.Empty,
                            Icon = "fa-circle",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Data/Nodes/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "Edges",
                            Title = "Interactions",
                            Description = string.Empty,
                            Icon = "fa-arrow-right",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Data/Edges/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "NodeCollections",
                            Title = "Protein collections",
                            Description = string.Empty,
                            Icon = "fa-folder",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Data/NodeCollections/Index"
                        }
                    }
                },
                new NavigationGroupViewModel
                {
                    Id = "Created",
                    Title = "My data",
                    Description = string.Empty,
                    Icon = "fa-user",
                    Color = "dark",
                    Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Created/Index",
                    NavigationPages = new List<NavigationPageViewModel>
                    {
                        new NavigationPageViewModel
                        {
                            Id = "Networks",
                            Title = "Networks",
                            Description = string.Empty,
                            Icon = "fa-desktop",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Created/Networks/Index"
                        },
                        new NavigationPageViewModel
                        {
                            Id = "ControlPaths",
                            Title = "Control paths",
                            Description = string.Empty,
                            Icon = "fa-gamepad",
                            Color = "light",
                            Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Created/ControlPaths/Index"
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Gets the updated navigation area for the content generic network pages.
        /// </summary>
        /// <param name="network">Represents the current network.</param>
        /// <returns>The navigation area for the network pages.</returns>
        public static NavigationAreaViewModel GetContentGenericNetworkNavigationArea(Network network)
        {
            // Get the corresponding navigation area.
            var navigationArea = ContentGenericNetworkNavigationArea;
            // Update the route ID.
            navigationArea.RouteId = network.Id;
            // Return the navigation area.
            return navigationArea;
        }

        /// <summary>
        /// Gets the updated navigation area for the content PPI network pages.
        /// </summary>
        /// <param name="network">Represents the current network.</param>
        /// <returns>The navigation area for the network pages.</returns>
        public static NavigationAreaViewModel GetContentPPINetworkNavigationArea(Network network)
        {
            // Get the corresponding navigation area.
            var navigationArea = ContentPPINetworkNavigationArea;
            // Update the route ID.
            navigationArea.RouteId = network.Id;
            // Return the navigation area.
            return navigationArea;
        }

        /// <summary>
        /// Gets the updated navigation area for the content generic analysis pages.
        /// </summary>
        /// <param name="analysis">Represents the current analysis.</param>
        /// <returns>The navigation area for the analysis pages.</returns>
        public static NavigationAreaViewModel GetContentGenericAnalysisNavigationArea(Analysis analysis)
        {
            // Get the corresponding navigation area.
            var navigationArea = ContentGenericAnalysisNavigationArea;
            // Update the route ID.
            navigationArea.RouteId = analysis.Id;
            // Return the navigation area.
            return navigationArea;
        }

        /// <summary>
        /// Gets the updated navigation area for the content PPI analysis pages.
        /// </summary>
        /// <param name="analysis">Represents the current analysis.</param>
        /// <returns>The navigation area for the analysis pages.</returns>
        public static NavigationAreaViewModel GetContentPPIAnalysisNavigationArea(Analysis analysis)
        {
            // Get the corresponding navigation area.
            var navigationArea = ContentPPIAnalysisNavigationArea;
            // Update the route ID.
            navigationArea.RouteId = analysis.Id;
            // Return the navigation area.
            return navigationArea;
        }
    }
}
