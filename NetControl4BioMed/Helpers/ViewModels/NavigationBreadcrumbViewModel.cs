namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents a navigation breadcrumb.
    /// </summary>
    public class NavigationBreadcrumbViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the breadcrumb.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title (displayed text) of the breadcrumb.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the destination link of the breadcrumb.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the route ID of the breadcrumb.
        /// </summary>
        public string RouteId { get; set; }

        /// <summary>
        /// Gets the navigation breadcrumb for the index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel NavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Index",
            Title = "Home",
            Link = "/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the account index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AccountNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Account",
            Title = "Account",
            Link = "/Account/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the account manage index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AccountManageNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Manage",
            Title = "Manage",
            Link = "/Account/Manage/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the account manage external logins index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AccountManageExternalLoginsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "ExternalLogins",
            Title = "External logins",
            Link = "/Account/Manage/ExternalLogins/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the account manage password index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AccountManagePasswordNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Password",
            Title = "Password",
            Link = "/Account/Manage/Password/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the account manage personal data index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AccountManagePersonalDataNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "PersonalData",
            Title = "Personal data",
            Link = "/Account/Manage/PersonalData/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the account manage profile index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AccountManageProfileNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Profile",
            Title = "Profile",
            Link = "/Account/Manage/Profile/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the account manage two-factor authentication index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AccountManageTwoFactorAuthenticationNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "TwoFactorAuthentication",
            Title = "Two-factor authentication",
            Link = "/Account/Manage/TwoFactorAuthentication/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Administration",
            Title = "Administration",
            Link = "/Administration/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration accounts index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationAccountsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Accounts",
            Title = "Accounts",
            Link = "/Administration/Accounts/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration accounts roles index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationAccountsRolesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Roles",
            Title = "Roles",
            Link = "/Administration/Accounts/Roles/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration accounts user roles index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationAccountsUserRolesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "UserRoles",
            Title = "User roles",
            Link = "/Administration/Accounts/UserRoles/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration accounts users index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationAccountsUsersNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Users",
            Title = "Users",
            Link = "/Administration/Accounts/Users/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration created index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationCreatedNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Created",
            Title = "Created",
            Link = "/Administration/Created/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration created analyses index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationCreatedAnalysesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Analyses",
            Title = "Analyses",
            Link = "/Administration/Created/Analyses/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration created networks index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationCreatedNetworksNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Networks",
            Title = "Networks",
            Link = "/Administration/Created/Networks/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration data index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDataNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Data",
            Title = "Data",
            Link = "/Administration/Data/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration data interactions index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDataInteractionsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Interactions",
            Title = "Interactions",
            Link = "/Administration/Data/Interactions/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration data protein collections index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDataProteinCollectionsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "ProteinCollections",
            Title = "Protein collections",
            Link = "/Administration/Data/ProteinCollections/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration data proteins index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDataProteinsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Proteins",
            Title = "Proteins",
            Link = "/Administration/Data/Proteins/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration databases index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDatabasesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Databases",
            Title = "Databases",
            Link = "/Administration/Databases/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration databases database interaction fields index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDatabasesDatabaseInteractionFieldsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseInteractionFields",
            Title = "Database interaction fields",
            Link = "/Administration/Databases/DatabaseInteractionFields/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration databases database protein fields index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDatabasesDatabaseProteinFieldsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseProteinFields",
            Title = "Database protein fields",
            Link = "/Administration/Databases/DatabaseProteinFields/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration databases databases index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDatabasesDatabasesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Databases",
            Title = "Databases",
            Link = "/Administration/Databases/Databases/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration created index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationOtherNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Other",
            Title = "Other",
            Link = "/Administration/Other/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration other background tasks index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationOtherBackgroundTasksNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "BackgroundTasks",
            Title = "Background tasks",
            Link = "/Administration/Other/BackgroundTasks/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration permissions index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationPermissionsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Permissions",
            Title = "Permissions",
            Link = "/Administration/Permissions/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration permissions database users index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationPermissionsDatabaseUsersNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseUsers",
            Title = "Database users",
            Link = "/Administration/Permissions/DatabaseUsers/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Relationships",
            Title = "Relationships",
            Link = "/Administration/Relationships/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships database interaction field interactions index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsDatabaseInteractionFieldInteractionsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseInteractionFieldInteractions",
            Title = "Database interaction field interactions",
            Link = "/Administration/Relationships/DatabaseInteractionFieldInteractions/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships database interactions index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsDatabaseInteractionsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseInteractions",
            Title = "Database interactions",
            Link = "/Administration/Relationships/DatabaseInteractions/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships database protein field proteins index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsDatabaseProteinFieldProteinsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseProteinFieldProteins",
            Title = "Database protein field proteins",
            Link = "/Administration/Relationships/DatabaseProteinFieldProteins/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships database proteins index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsDatabaseProteinsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseProteins",
            Title = "Database proteins",
            Link = "/Administration/Relationships/DatabaseProteins/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships interaction proteins index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsInteractionProteinsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "InteractionProteins",
            Title = "Interaction proteins",
            Link = "/Administration/Relationships/InteractionProteins/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships protein collection proteins index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsProteinCollectionProteinsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "ProteinCollectionProteins",
            Title = "Protein collection proteins",
            Link = "/Administration/Relationships/ProteinCollectionProteins/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships protein collection types index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsProteinCollectionTypesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "ProteinCollectionTypes",
            Title = "Protein collection types",
            Link = "/Administration/Relationships/ProteinCollectionTypes/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the available data index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AvailableDataNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "AvailableData",
            Title = "Available data",
            Link = "/AvailableData/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the available data data index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AvailableDataDataNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Data",
            Title = "Data",
            Link = "/AvailableData/Data/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the available data data interactions index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AvailableDataDataInteractionsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Interactions",
            Title = "Interactions",
            Link = "/AvailableData/Data/Interactions/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the available data data protein collections index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AvailableDataDataProteinCollectionsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "ProteinCollections",
            Title = "Protein collections",
            Link = "/AvailableData/Data/ProteinCollections/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the available data data proteins index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AvailableDataDataProteinsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Proteins",
            Title = "Proteins",
            Link = "/AvailableData/Data/Proteins/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the available data databases index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AvailableDataDatabasesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Databases",
            Title = "Databases",
            Link = "/AvailableData/Databases/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the available data databases database interaction fields index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AvailableDataDatabasesDatabaseInteractionFieldsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseInteractionFields",
            Title = "Database interaction fields",
            Link = "/AvailableData/Databases/DatabaseInteractionFields/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the available data databases database protein fields index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AvailableDataDatabasesDatabaseProteinFieldsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseProteinFields",
            Title = "Database protein fields",
            Link = "/AvailableData/Databases/DatabaseProteinFields/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the available data databases databases index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AvailableDataDatabasesDatabasesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Databases",
            Title = "Databases",
            Link = "/AvailableData/Databases/Databases/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the public data index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel PublicDataNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "PublicData",
            Title = "Public data",
            Link = "/PublicData/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the public data networks index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel PublicDataNetworksNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Networks",
            Title = "Networks",
            Link = "/PublicData/Networks/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the public data analyses index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel PublicDataAnalysesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Analyses",
            Title = "Analyses",
            Link = "/PublicData/Analyses/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the private data index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel PrivateDataNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "PrivateData",
            Title = "Private data",
            Link = "/PrivateData/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the private data networks index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel PrivateDataNetworksNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Networks",
            Title = "Networks",
            Link = "/PrivateData/Networks/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the private data analyses index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel PrivateDataAnalysesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Analyses",
            Title = "Analyses",
            Link = "/PrivateData/Analyses/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the created data index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel CreatedDataNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "CreatedData",
            Title = "Created data",
            Link = "/CreatedData/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the created data networks index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel CreatedDataNetworksNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Networks",
            Title = "Networks",
            Link = "/CreatedData/Networks/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the created data networks details index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel CreatedDataNetworksDetailsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Details",
            Title = "Network details",
            Link = "/CreatedData/Networks/Details/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the created data analyses index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel CreatedDataAnalysesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Analyses",
            Title = "Analyses",
            Link = "/CreatedData/Analyses/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the created data analyses details index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel CreatedDataAnalysesDetailsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Details",
            Title = "Analysis details",
            Link = "/CreatedData/Analyses/Details/Index"
        };

        /// <summary>
        /// Gets the updated navigation breadcrumb for the created data networks details pages.
        /// </summary>
        /// <param name="networkId">The ID of the current network.</param>
        /// <returns>The navigation area for the network pages.</returns>
        public static NavigationBreadcrumbViewModel GetCreatedDataNetworksDetailsNavigationBreadcrumb(string networkId = null)
        {
            // Get the corresponding navigation breadcrumb.
            var navigationBreadcrumb = CreatedDataNetworksDetailsNavigationBreadcrumb;
            // Update the route ID.
            navigationBreadcrumb.RouteId = networkId;
            // Return the breadcrumb area.
            return navigationBreadcrumb;
        }

        /// <summary>
        /// Gets the updated navigation breadcrumb for the created data analyses details pages.
        /// </summary>
        /// <param name="analysisId">The ID of the current analysis.</param>
        /// <returns>The navigation area for the analysis pages.</returns>
        public static NavigationBreadcrumbViewModel GetCreatedDataAnalysesDetailsNavigationBreadcrumb(string analysisId = null)
        {
            // Get the corresponding breadcrumb area.
            var navigationBreadcrumb = CreatedDataAnalysesDetailsNavigationBreadcrumb;
            // Update the route ID.
            navigationBreadcrumb.RouteId = analysisId;
            // Return the breadcrumb area.
            return navigationBreadcrumb;
        }
    }
}
