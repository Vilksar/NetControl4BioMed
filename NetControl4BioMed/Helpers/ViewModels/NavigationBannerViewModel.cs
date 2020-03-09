using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            Icon = "fa-user",
            Color = "info"
        };

        /// <summary>
        /// Gets the navigation banner for the administration pages.
        /// </summary>
        public static NavigationBannerViewModel AdministrationNavigationBanner { get; } = new NavigationBannerViewModel
        {
            Id = "Administration",
            Title = "Administration",
            Icon = "fa-cog",
            Color = "primary"
        };

        /// <summary>
        /// Gets the navigation banner for the content pages.
        /// </summary>
        public static NavigationBannerViewModel ContentNavigationBanner { get; } = new NavigationBannerViewModel
        {
            Id = "Content",
            Title = "Content",
            Icon = "fa-box-open",
            Color = "success"
        };

        /// <summary>
        /// Gets the navigation banner for the network pages.
        /// </summary>
        public static NavigationBannerViewModel NetworkNavigationBanner { get; } = new NavigationBannerViewModel
        {
            Id = "Network",
            Title = "Network",
            Icon = "fa-share-alt",
            Color = "dark"
        };

        /// <summary>
        /// Gets the navigation banner for the network pages.
        /// </summary>
        public static NavigationBannerViewModel AnalysisNavigationBanner { get; } = new NavigationBannerViewModel
        {
            Id = "Analysis",
            Title = "Analysis",
            Icon = "fa-desktop",
            Color = "dark"
        };
    }
}
