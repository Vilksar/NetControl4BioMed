using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace NetControl4BioMed.Pages.AvailableData.Created.Analyses.Details.Results.ControlPaths
{
    public class DownloadModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IReCaptchaChecker _reCaptchaChecker;
        private readonly IServiceProvider _serviceProvider;

        public DownloadModel(UserManager<User> userManager, ApplicationDbContext context, IReCaptchaChecker reCaptchaChecker, IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _context = context;
            _reCaptchaChecker = reCaptchaChecker;
            _serviceProvider = serviceProvider;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            [RegularExpression("txt|sif|json|cyjs|xlsx", ErrorMessage = "The value is not valid.")]
            public string FileFormat { get; set; }

            public string ReCaptchaToken { get; set; }

            public IEnumerable<string> Ids { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Analysis Analysis { get; set; }

            public HashSet<Protein> SourceProteins { get; set; }

            public IEnumerable<ControlPath> Items { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(IEnumerable<string> ids)
        {
            // Check if there aren't any IDs provided.
            if (ids == null || !ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Analyses/Index");
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the item with the provided ID.
            var items = _context.ControlPaths
                .Where(item => item.Analysis.IsPublic || (user != null && item.Analysis.AnalysisUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => ids.Contains(item.Id));
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No control paths have been found with the provided ID, or you don't have access to them.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .Select(item => item.Analysis)
                    .First(),
                SourceProteins = items
                    .Select(item => item.Analysis.AnalysisProteins)
                    .SelectMany(item => item)
                    .Where(item => item.Type == AnalysisProteinType.Source)
                    .Select(item => item.Protein)
                    .Distinct()
                    .ToHashSet(),
                Items = items
                    .Include(item => item.Paths)
                        .ThenInclude(item => item.PathProteins)
                            .ThenInclude(item => item.Protein)
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if there aren't any IDs provided.
            if (Input.Ids == null || !Input.Ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Analyses/Index");
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Get the item with the provided ID.
            var items = _context.ControlPaths
                .Where(item => item.Analysis.IsPublic || (user != null && item.Analysis.AnalysisUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => Input.Ids.Contains(item.Id));
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No control paths have been found with the provided ID, or you don't have access to them.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .Select(item => item.Analysis)
                    .First(),
                SourceProteins = items
                    .Select(item => item.Analysis.AnalysisProteins)
                    .SelectMany(item => item)
                    .Where(item => item.Type == AnalysisProteinType.Source)
                    .Select(item => item.Protein)
                    .ToHashSet(),
                Items = items
                    .Include(item => item.Paths)
                        .ThenInclude(item => item.PathProteins)
                            .ThenInclude(item => item.Protein)
            };
            // Check if the reCaptcha is valid.
            if (!await _reCaptchaChecker.IsValid(Input.ReCaptchaToken))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The reCaptcha verification failed.");
                // Return the page.
                return Page();
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Return the streamed file.
            return new FileCallbackResult(MediaTypeNames.Application.Zip, async (zipStream, _) =>
            {
                // Define a new ZIP archive.
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
                // Check if the overview file should be added.
                if (true)
                {
                    // Create a new entry in the archive and open it.
                    using var stream = archive.CreateEntry($"Control-Paths-List.txt", CompressionLevel.Fastest).Open();
                    // Write to the entry the corresponding file content.
                    await ControlPathExtensions.WriteToStreamOverviewTextFileContent(View.Items.Select(item => item.Id), stream, _serviceProvider, HttpContext.Request.Scheme, HttpContext.Request.Host);
                }
                // Check which should be the format of the files within the archive.
                if (Input.FileFormat == "txt")
                {
                    // Go over each of the analyses to download.
                    foreach (var controlPath in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Control-Path-{controlPath.Analysis.Name.Replace(" ", "-")}-{controlPath.Id}.txt", CompressionLevel.Fastest).Open();
                        // Write to the entry the corresponding file content.
                        await controlPath.WriteToStreamTxtFileContent(stream, _serviceProvider);
                    }
                }
                else if (Input.FileFormat == "sif")
                {
                    // Go over each of the analyses to download.
                    foreach (var controlPath in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Control-Path-{controlPath.Analysis.Name.Replace(" ", "-")}-{controlPath.Id}.sif", CompressionLevel.Fastest).Open();
                        // Write to the entry the corresponding file content.
                        await controlPath.WriteToStreamSifFileContent(stream, _serviceProvider);
                    }
                }
                else if (Input.FileFormat == "json")
                {
                    // Go over each of the analyses to download.
                    foreach (var controlPath in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Control-Path-{controlPath.Analysis.Name.Replace(" ", "-")}-{controlPath.Id}.json", CompressionLevel.Fastest).Open();
                        // Write to the entry the corresponding file content.
                        await controlPath.WriteToStreamJsonFileContent(stream, _serviceProvider);
                    }
                }
                else if (Input.FileFormat == "cyjs")
                {
                    // Go over each of the analyses to download.
                    foreach (var controlPath in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Control-Path-{controlPath.Analysis.Name.Replace(" ", "-")}-{controlPath.Id}.cyjs", CompressionLevel.Fastest).Open();
                        // Write to the entry the corresponding file content.
                        await controlPath.WriteToStreamCyjsFileContent(stream, _serviceProvider);
                    }
                }
                else if (Input.FileFormat == "xlsx")
                {
                    // Go over each of the analyses to download.
                    foreach (var controlPath in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Control-Path-{controlPath.Analysis.Name.Replace(" ", "-")}-{controlPath.Id}.xlsx", CompressionLevel.Fastest).Open();
                        // Write to the entry the corresponding file content.
                        await controlPath.WriteToStreamXlsxFileContent(user, stream, _serviceProvider);
                    }
                }
            })
            {
                FileDownloadName = $"NetControl4BioMed-Control-Paths.zip"
            };
        }
    }
}
