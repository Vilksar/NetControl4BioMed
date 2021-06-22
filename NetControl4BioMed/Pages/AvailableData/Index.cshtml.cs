using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace NetControl4BioMed.Pages.AvailableData
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Dictionary<string, int?> PublicItemCount { get; set; }
        }

        public IActionResult OnGet()
        {
            // Get the data from configuration.
            var publicItemCount = _configuration
                .GetSection("Data")
                .GetSection("ItemCount")
                .GetSection("Public")
                .GetChildren()
                .ToDictionary(item => item.Key, item => int.TryParse(item.Value, out var result) ? (int?)result : null);
            // Define the view.
            View = new ViewModel
            {
                PublicItemCount = publicItemCount
            };
            // Return the page.
            return Page();
        }
    }
}
