using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NetControl4BioMed.Pages.Administration
{
    [Authorize(Roles = "Administrator")]
    public class HangfireModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Return the page.
            return Page();
        }

        public IActionResult OnPost()
        {
            // Redirect to the Hangfire dashboard.
            return LocalRedirect("/Hangfire");
        }
    }
}
