using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NetControl4BioMed.Pages.Identity
{
    [AllowAnonymous]
    public class AccessDeniedModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Return the page.
            return Page();
        }
    }
}
