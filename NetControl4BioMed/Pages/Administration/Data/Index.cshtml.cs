using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NetControl4BioMed.Pages.Administration.Data
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Redirect to the index page.
            return RedirectToPage("/Administration/Index");
        }
    }
}
