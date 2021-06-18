using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace NetControl4BioMed.Pages
{
    [AllowAnonymous]
    public class PrivacyPolicyModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Return the page.
            return Page();
        }
    }
}
