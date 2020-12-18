using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Services;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.Generic.Created.Analyses.Details.Created.ControlPaths
{
    public class DownloadModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public DownloadModel(UserManager<User> userManager, ApplicationDbContext context, LinkGenerator linkGenerator, IReCaptchaChecker reCaptchaChecker)
        {
            _userManager = userManager;
            _context = context;
            _linkGenerator = linkGenerator;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            [RegularExpression("Text|Json|CytoscapeJson|Excel", ErrorMessage = "The value is not valid.")]
            public string FileFormat { get; set; }

            public string ReCaptchaToken { get; set; }

            public IEnumerable<string> Ids { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Analysis Analysis { get; set; }

            public HashSet<Node> SourceNodes { get; set; }

            public IEnumerable<ControlPath> Items { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(IEnumerable<string> ids)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there aren't any IDs provided.
            if (ids == null || !ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Analyses/Index");
            }
            // Get the item with the provided ID.
            var items = _context.ControlPaths
                .Where(item => item.Analysis.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => item.Analysis.IsPublic || item.Analysis.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => ids.Contains(item.Id));
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No control paths have been found with the provided ID, or you don't have access to them.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .Select(item => item.Analysis)
                    .First(),
                SourceNodes = items
                    .Select(item => item.Analysis.AnalysisNodes)
                    .SelectMany(item => item)
                    .Where(item => item.Type == AnalysisNodeType.Source)
                    .Select(item => item.Node)
                    .Distinct()
                    .ToHashSet(),
                Items = items
                    .Include(item => item.Paths)
                        .ThenInclude(item => item.PathNodes)
                            .ThenInclude(item => item.Node)
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there aren't any IDs provided.
            if (Input.Ids == null || !Input.Ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Analyses/Index");
            }
            // Get the item with the provided ID.
            var items = _context.ControlPaths
                .Where(item => item.Analysis.AnalysisDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Where(item => item.Analysis.IsPublic || item.Analysis.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => Input.Ids.Contains(item.Id));
            // Check if there was no item found.
            if (items == null || !items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No control paths have been found with the provided ID, or you don't have access to them.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Analysis = items
                    .Select(item => item.Analysis)
                    .First(),
                SourceNodes = items
                    .Select(item => item.Analysis.AnalysisNodes)
                    .SelectMany(item => item)
                    .Where(item => item.Type == AnalysisNodeType.Source)
                    .Select(item => item.Node)
                    .ToHashSet(),
                Items = items
                    .Include(item => item.Paths)
                        .ThenInclude(item => item.PathNodes)
                            .ThenInclude(item => item.Node)
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
                // Check which should be the format of the files within the archive.
                if (Input.FileFormat == "Text")
                {
                    // Go over each of the analyses to download.
                    foreach (var controlPath in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Control-Path-{controlPath.Analysis.Name.Replace(" ", "-")}-{controlPath.Id}.txt", CompressionLevel.Fastest).Open();
                        // Define the stream writer for the file.
                        using var streamWriter = new StreamWriter(stream);
                        // Get the required data.
                        var data = string.Join("\n", _context.ControlPaths
                            .Where(item => item == controlPath)
                            .Select(item => item.Paths)
                            .SelectMany(item => item)
                            .Select(item => item.PathNodes.Where(item1 => item1.Type == PathNodeType.None).OrderBy(item1 => item1.Index).Select(item1 => item1.Node.Name))
                            .Select(item => string.Join("\t", item)));
                        // Write the corresponding to the file.
                        await streamWriter.WriteAsync(data);
                    }
                }
                else if (Input.FileFormat == "Json")
                {
                    // Define the JSON serializer options.
                    var jsonSerializerOptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    // Go over each of the analyses to download.
                    foreach (var controlPath in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Control-Path-{controlPath.Analysis.Name.Replace(" ", "-")}-{controlPath.Id}.json", CompressionLevel.Fastest).Open();
                        // Get the required data.
                        var data = new
                        {
                            Id = controlPath.Id,
                            Analysis = _context.Analyses
                                .Where(item => item.ControlPaths.Any(item1 => item1 == controlPath))
                                .Select(item => new
                                {
                                    Id = item.Id,
                                    Name = item.Name,
                                    Description = item.Description,
                                    Algorithm = item.Algorithm.GetDisplayName()
                                })
                                .FirstOrDefault(),
                            UniqueControlNodes = _context.Paths
                                .Where(item1 => item1.ControlPath == controlPath)
                                .Select(item1 => item1.PathNodes)
                                .SelectMany(item1 => item1)
                                .Where(item1 => item1.Type == PathNodeType.Source)
                                .Select(item1 => item1.Node)
                                .AsEnumerable()
                                .GroupBy(item1 => item1)
                                .Select(item1 => new
                                {
                                    Id = item1.Key.Id,
                                    Name = item1.Key.Name,
                                    Count = item1.Count()
                                }),
                            ControlNodes = _context.Paths
                                .Where(item1 => item1.ControlPath == controlPath)
                                .Select(item1 => new
                                {
                                    Id = item1.Id,
                                    SourceNode = item1.PathNodes
                                        .Where(item2 => item2.Type == PathNodeType.Source)
                                        .Select(item2 => item2.Node)
                                        .FirstOrDefault(),
                                    TargetNode = item1.PathNodes
                                        .Where(item2 => item2.Type == PathNodeType.Target)
                                        .Select(item2 => item2.Node)
                                        .FirstOrDefault(),
                                })
                                .Where(item1 => item1.SourceNode != null && item1.TargetNode != null)
                                .AsEnumerable()
                                .Select(item1 => new
                                {
                                    SourceNode = new
                                    {
                                        Id = item1.SourceNode.Id,
                                        Name = item1.SourceNode.Name
                                    },
                                    TargetNode = new
                                    {
                                        Id = item1.TargetNode.Id,
                                        Name = item1.TargetNode.Name
                                    }
                                })
                        };
                        // Write the data corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                }
                else if (Input.FileFormat == "CytoscapeJson")
                {
                    // Define the JSON serializer options for all of the returned files.
                    var jsonSerializerOptions = new JsonSerializerOptions
                    {
                        IgnoreNullValues = true,
                        WriteIndented = true
                    };
                    // Go over each of the analyses to download.
                    foreach (var controlPath in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Control-Path-{controlPath.Analysis.Name.Replace(" ", "-")}-{controlPath.Id}.json", CompressionLevel.Fastest).Open();
                        // Write the data corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, controlPath.GetCytoscapeViewModel(HttpContext, _linkGenerator, _context), jsonSerializerOptions);
                    }
                }
                else if (Input.FileFormat == "Excel")
                {
                    // Go over each of the analyses to download.
                    foreach (var controlPath in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Control-Path-{controlPath.Analysis.Name.Replace(" ", "-")}-{controlPath.Id}.xlsx", CompressionLevel.Fastest).Open();
                        // Get the required data.
                        var analysis = _context.Analyses
                            .Where(item => item.ControlPaths.Any(item1 => item1 == controlPath))
                            .Select(item => new
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Description = item.Description,
                                Algorithm = item.Algorithm.GetDisplayName()
                            })
                            .FirstOrDefault();
                        // Define the rows in the first sheet.
                        var worksheet1Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Analysis ID", "Analysis name", "Analysis description", "Analysis algorithm" },
                            new List<string> { controlPath.Id, analysis?.Id ?? string.Empty, analysis?.Name ?? string.Empty, analysis?.Description ?? string.Empty, analysis?.Algorithm ?? string.Empty }
                        };
                        // Define the rows in the second sheet.
                        var worksheet2Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Name", "Count" }
                        }
                        .Concat(_context.Paths
                            .Where(item1 => item1.ControlPath == controlPath)
                            .Select(item1 => item1.PathNodes)
                            .SelectMany(item1 => item1)
                            .Where(item1 => item1.Type == PathNodeType.Source)
                            .Select(item1 => item1.Node)
                            .AsEnumerable()
                            .GroupBy(item1 => item1)
                            .Select(item1 => new List<string> { item1.Key.Id, item1.Key.Name, item1.Count().ToString() }))
                        .ToList();
                        // Define the rows in the third sheet.
                        var worksheet3Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Source node ID", "Source node name", "...", "Target node ID", "Target node name" }
                        }
                        .Concat(_context.Paths
                            .Where(item1 => item1.ControlPath == controlPath)
                            .Select(item1 => new
                            {
                                Id = item1.Id,
                                SourceNode = item1.PathNodes
                                    .Where(item2 => item2.Type == PathNodeType.Source)
                                    .Select(item2 => item2.Node)
                                    .FirstOrDefault(),
                                TargetNode = item1.PathNodes
                                    .Where(item2 => item2.Type == PathNodeType.Target)
                                    .Select(item2 => item2.Node)
                                    .FirstOrDefault(),
                            })
                            .Where(item1 => item1.SourceNode != null && item1.TargetNode != null)
                            .AsEnumerable()
                            .Select(item1 => new List<string> { item1.Id, item1.SourceNode.Id, item1.SourceNode.Name, item1.TargetNode.Id, item1.TargetNode.Name } ))
                        .ToList();
                        // Define the stream for the file.
                        var fileStream = new MemoryStream();
                        // Define the Excel file.
                        using SpreadsheetDocument document = SpreadsheetDocument.Create(fileStream, SpreadsheetDocumentType.Workbook);
                        // Definte a new workbook part.
                        var workbookPart = document.AddWorkbookPart();
                        workbookPart.Workbook = new Workbook();
                        var worksheets = workbookPart.Workbook.AppendChild(new Sheets());
                        // Define the first worksheet.
                        var worksheet1Part = workbookPart.AddNewPart<WorksheetPart>();
                        var worksheet1Data = new SheetData();
                        var worksheet1 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet1Part), SheetId = 1, Name = "Details" };
                        worksheet1Part.Worksheet = new Worksheet(worksheet1Data);
                        worksheet1Data.Append(worksheet1Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
                        worksheets.Append(worksheet1);
                        // Define the second worksheet.
                        var worksheet2Part = workbookPart.AddNewPart<WorksheetPart>();
                        var worksheet2Data = new SheetData();
                        var worksheet2 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet2Part), SheetId = 2, Name = "Nodes" };
                        worksheet2Part.Worksheet = new Worksheet(worksheet2Data);
                        worksheet2Data.Append(worksheet2Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
                        worksheets.Append(worksheet2);
                        // Define the third worksheet.
                        var worksheet3Part = workbookPart.AddNewPart<WorksheetPart>();
                        var worksheet3Data = new SheetData();
                        var worksheet3 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet3Part), SheetId = 3, Name = "Edges" };
                        worksheet3Part.Worksheet = new Worksheet(worksheet3Data);
                        worksheet3Data.Append(worksheet3Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
                        worksheets.Append(worksheet3);
                        // Close the document.
                        document.Close();
                        // Reset the stream position.
                        fileStream.Position = 0;
                        // Copy it to the archive stream.
                        await fileStream.CopyToAsync(stream);
                    }
                }
            })
            {
                FileDownloadName = $"NetControl4BioMed-Control-Paths.zip"
            };
        }
    }
}
