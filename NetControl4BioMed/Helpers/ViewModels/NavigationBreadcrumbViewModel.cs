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
        /// Gets the navigation breadcrumb for the content index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Content",
            Title = "Content",
            Link = "/Content/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content created index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentCreatedNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Created",
            Title = "Created",
            Link = "/Content/Created/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content created analyses index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentCreatedAnalysesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Analyses",
            Title = "Analyses",
            Link = "/Content/Created/Analyses/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content created networks index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentCreatedNetworksNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Networks",
            Title = "Networks",
            Link = "/Content/Created/Networks/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content data index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDataNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Data",
            Title = "Data",
            Link = "/Content/Data/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content data edges index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDataEdgesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Edges",
            Title = "Edges",
            Link = "/Content/Data/Edges/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content data node collections index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDataNodeCollectionsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "NodeCollections",
            Title = "Node collections",
            Link = "/Content/Data/NodeCollections/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content data nodes index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDataNodesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Nodes",
            Title = "Nodes",
            Link = "/Content/Data/Nodes/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content databases index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabasesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Databases",
            Title = "Databases",
            Link = "/Content/Databases/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content databases database edge fields index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabasesDatabaseEdgeFieldsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseEdgeFields",
            Title = "Database edge fields",
            Link = "/Content/Databases/DatabaseEdgeFields/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content databases database node fields index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabasesDatabaseNodeFieldsNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseNodeFields",
            Title = "Database node fields",
            Link = "/Content/Databases/DatabaseNodeFields/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content databases databases index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabasesDatabasesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "Databases",
            Title = "Databases",
            Link = "/Content/Databases/Databases/Index"
        };

        /// <summary>
        /// Gets the navigation breadcrumb for the content databases database types index page.
        /// </summary>
        public static NavigationBreadcrumbViewModel ContentDatabasesDatabaseTypesNavigationBreadcrumb { get; } = new NavigationBreadcrumbViewModel
        {
            Id = "DatabaseTypes",
            Title = "Database types",
            Link = "/Content/Databases/DatabaseTypes/Index"
        };
    }
}
