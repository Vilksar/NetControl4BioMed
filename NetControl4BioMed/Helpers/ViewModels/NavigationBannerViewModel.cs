namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents a navigation banner, to be displayed on the top of the page.
    /// </summary>
    public class NavigationBannerViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the banner.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title (displayed text) of the banner.
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
        /// Gets the navigation banner for the account pages.
        /// </summary>
        public static NavigationBannerViewModel AccountNavigationBanner { get; } = new NavigationBannerViewModel
        {
            Id = "Account",
            Title = "Account",
            Icon = "fa-user-circle",
            Color = "info"
        };

        /// <summary>
        /// Gets the navigation banner for the administration pages.
        /// </summary>
        public static NavigationBannerViewModel AdministrationNavigationBanner { get; } = new NavigationBannerViewModel
        {
            Id = "Administration",
            Title = "Administration",
            Icon = "fa-toolbox",
            Color = "primary"
        };

        /// <summary>
        /// Gets the navigation banner for the available data pages.
        /// </summary>
        public static NavigationBannerViewModel AvailableDataNavigationBanner { get; } = new NavigationBannerViewModel
        {
            Id = "AvailableData",
            Title = "Available data",
            Icon = "fa-database",
            Color = "primary"
        };

        /// <summary>
        /// Gets the navigation banner for the private data pages.
        /// </summary>
        public static NavigationBannerViewModel PrivateDataNavigationBanner { get; } = new NavigationBannerViewModel
        {
            Id = "PrivateData",
            Title = "Private data",
            Icon = "fa-user",
            Color = "success"
        };

        /// <summary>
        /// Gets the navigation banner for the public data pages.
        /// </summary>
        public static NavigationBannerViewModel PublicDataNavigationBanner { get; } = new NavigationBannerViewModel
        {
            Id = "PublicData",
            Title = "Public data",
            Icon = "fa-users",
            Color = "success"
        };

        /// <summary>
        /// Gets the navigation banner for the created data networks pages.
        /// </summary>
        public static NavigationBannerViewModel CreatedDataNetworksNavigationBanner { get; } = new NavigationBannerViewModel
        {
            Id = "Networks",
            Title = "Networks",
            Icon = "fa-share-alt",
            Color = "success"
        };

        /// <summary>
        /// Gets the navigation banner for the created data analyses pages.
        /// </summary>
        public static NavigationBannerViewModel CreatedDataAnalysesNavigationBanner { get; } = new NavigationBannerViewModel
        {
            Id = "Analyses",
            Title = "Analyses",
            Icon = "fa-code-branch",
            Color = "success"
        };
    }
}
