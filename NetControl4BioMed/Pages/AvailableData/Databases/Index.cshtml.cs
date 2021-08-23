using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NetControl4BioMed.Pages.AvailableData.Databases
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Redirect to the index page.
            return RedirectToPage("/AvailableData/Index");
        }
    }
}
