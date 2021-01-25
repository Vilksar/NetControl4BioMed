using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// Gets the navigation breadcrumb for the administration data edges index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDataEdgesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Edges",
            Title = "Edges",
            Link = "/Administration/Data/Edges/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration data node collections index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDataNodeCollectionsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "NodeCollections",
            Title = "Node collections",
            Link = "/Administration/Data/NodeCollections/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration data nodes index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDataNodesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Nodes",
            Title = "Nodes",
            Link = "/Administration/Data/Nodes/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration data samples index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDataSamplesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Samples",
            Title = "Samples",
            Link = "/Administration/Data/Samples/Index"
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
        /// Gets the navigation breadcrumb for the administration databases database edge fields index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDatabasesDatabaseEdgeFieldsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseEdgeFields",
            Title = "Database edge fields",
            Link = "/Administration/Databases/DatabaseEdgeFields/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration databases database node fields index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDatabasesDatabaseNodeFieldsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseNodeFields",
            Title = "Database node fields",
            Link = "/Administration/Databases/DatabaseNodeFields/Index"
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
        /// Gets the navigation breadcrumb for the administration databases database types index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationDatabasesDatabaseTypesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseTypes",
            Title = "Database types",
            Link = "/Administration/Databases/DatabaseTypes/Index"
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
        /// Gets the navigation breadcrumb for the administration permissions database user invitations index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationPermissionsDatabaseUserInvitationsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseUserInvitations",
            Title = "Database user invitations",
            Link = "/Administration/Permissions/DatabaseUserInvitations/Index"
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
        /// Gets the navigation breadcrumb for the administration relationships database edge field edges index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsDatabaseEdgeFieldEdgesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseEdgeFieldEdges",
            Title = "Database edge field edges",
            Link = "/Administration/Relationships/DatabaseEdgeFieldEdges/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships database edges index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsDatabaseEdgesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseEdges",
            Title = "Database edges",
            Link = "/Administration/Relationships/DatabaseEdges/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships database node field nodes index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsDatabaseNodeFieldNodesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseNodeFieldNodes",
            Title = "Database node field nodes",
            Link = "/Administration/Relationships/DatabaseNodeFieldNodes/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships database nodes index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsDatabaseNodesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseNodes",
            Title = "Database nodes",
            Link = "/Administration/Relationships/DatabaseNodes/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships edge nodes index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsEdgeNodesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "EdgeNodes",
            Title = "Edge nodes",
            Link = "/Administration/Relationships/EdgeNodes/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships node collection databases index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsNodeCollectionDatabasesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "NodeCollectionDatabases",
            Title = "Node collection databases",
            Link = "/Administration/Relationships/NodeCollectionDatabases/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships node collection nodes index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsNodeCollectionNodesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "NodeCollectionNodes",
            Title = "Node collection nodes",
            Link = "/Administration/Relationships/NodeCollectionNodes/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships node collection types index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsNodeCollectionTypesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "NodeCollectionTypes",
            Title = "Node collection types",
            Link = "/Administration/Relationships/NodeCollectionTypes/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships sample databases index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsSampleDatabasesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "SampleDatabases",
            Title = "Sample databases",
            Link = "/Administration/Relationships/SampleDatabases/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the administration relationships sample types index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel AdministrationRelationshipsSampleTypesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "SampleTypes",
            Title = "Sample types",
            Link = "/Administration/Relationships/SampleTypes/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Content",
            Title = "Content",
            Link = "/Content/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content database types index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseTypes",
            Title = "Database types",
            Link = "/Content/DatabaseTypes/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content generic index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesGenericNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Generic",
            Title = "Generic",
            Link = "/Content/DatabaseTypes/Generic/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content generic created index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesGenericCreatedNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Created",
            Title = "Created",
            Link = "/Content/DatabaseTypes/Generic/Created/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content generic created analyses index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesGenericCreatedAnalysesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Analyses",
            Title = "Analyses",
            Link = "/Content/DatabaseTypes/Generic/Created/Analyses/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content generic created analyses details index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesGenericCreatedAnalysesDetailsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Details",
            Title = "Details",
            Link = "/Content/DatabaseTypes/Generic/Created/Analyses/Details/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content generic created networks index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesGenericCreatedNetworksNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Networks",
            Title = "Networks",
            Link = "/Content/DatabaseTypes/Generic/Created/Networks/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content generic created network details index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesGenericCreatedNetworksDetailsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Details",
            Title = "Details",
            Link = "/Content/DatabaseTypes/Generic/Created/Networks/Details/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content generic data index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesGenericDataNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Data",
            Title = "Data",
            Link = "/Content/DatabaseTypes/Generic/Data/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content generic data nodes index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesGenericDataNodesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Nodes",
            Title = "Nodes",
            Link = "/Content/DatabaseTypes/Generic/Data/Nodes/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content generic data edges index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesGenericDataEdgesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Edges",
            Title = "Edges",
            Link = "/Content/DatabaseTypes/Generic/Data/Edges/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content PPI index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPINavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "PPI",
            Title = "PPI",
            Link = "/Content/DatabaseTypes/PPI/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content created index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPICreatedNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Created",
            Title = "Created",
            Link = "/Content/DatabaseTypes/PPI/Created/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content created analyses index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPICreatedAnalysesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Analyses",
            Title = "Analyses",
            Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content PPI created analyses details index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPICreatedAnalysesDetailsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Details",
            Title = "Details",
            Link = "/Content/DatabaseTypes/PPI/Created/Analyses/Details/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content created networks index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPICreatedNetworksNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Networks",
            Title = "Networks",
            Link = "/Content/DatabaseTypes/PPI/Created/Networks/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content PPI created network details index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPICreatedNetworksDetailsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Details",
            Title = "Details",
            Link = "/Content/DatabaseTypes/PPI/Created/Networks/Details/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content data index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPIDataNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Data",
            Title = "Data",
            Link = "/Content/DatabaseTypes/PPI/Data/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content data edges index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPIDataEdgesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Edges",
            Title = "Edges",
            Link = "/Content/DatabaseTypes/PPI/Data/Edges/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content data source node collections index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPIDataSourceNodeCollectionsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "SourceNodeCollections",
            Title = "Source node collections",
            Link = "/Content/DatabaseTypes/PPI/Data/SourceNodeCollections/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content data target node collections index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPIDataTargetNodeCollectionsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "TargetNodeCollections",
            Title = "Target node collections",
            Link = "/Content/DatabaseTypes/PPI/Data/TargetNodeCollections/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content data nodes index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPIDataNodesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Nodes",
            Title = "Nodes",
            Link = "/Content/DatabaseTypes/PPI/Data/Nodes/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content databases index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPIDatabasesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Databases",
            Title = "Databases",
            Link = "/Content/DatabaseTypes/PPI/Databases/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content databases database edge fields index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPIDatabasesDatabaseEdgeFieldsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseEdgeFields",
            Title = "Database edge fields",
            Link = "/Content/DatabaseTypes/PPI/Databases/DatabaseEdgeFields/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content databases database node fields index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPIDatabasesDatabaseNodeFieldsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseNodeFields",
            Title = "Database node fields",
            Link = "/Content/DatabaseTypes/PPI/Databases/DatabaseNodeFields/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content databases databases index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabaseTypesPPIDatabasesDatabasesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Databases",
            Title = "Databases",
            Link = "/Content/DatabaseTypes/PPI/Databases/Databases/Index"
        };
    }
}
