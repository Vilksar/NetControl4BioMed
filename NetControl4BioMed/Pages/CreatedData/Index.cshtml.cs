using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NetControl4BioMed.Pages.CreatedData
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Check if the user is not authenticated.
            if (!User.Identity.IsAuthenticated)
            {
                // Redirect to the public data.
                return RedirectToPage("/PublicData/Index");
            }
            // Redirect to the private data.
            return RedirectToPage("/PrivateData/Index");
        }
    }
}
