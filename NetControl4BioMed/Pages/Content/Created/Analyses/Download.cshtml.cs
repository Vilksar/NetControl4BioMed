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
using NetControl4BioMed.Helpers.Services;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Content.Created.Analyses
{
    [Authorize]
    public class DownloadModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public DownloadModel(UserManager<User> userManager, ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _userManager = userManager;
            _context = context;
            _linkGenerator = linkGenerator;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            [RegularExpression("Text|Json|CytoscapeJson|Excel", ErrorMessage = "The value is not valid.")]
            public string FileFormat { get; set; }

            public IEnumerable<string> Ids { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<Analysis> Items { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(IEnumerable<string> ids)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if there aren't any IDs provided.
            if (ids == null || !ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Analyses
                    .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                    .Where(item => ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No analyses have been found with the provided IDs, or you don't have access to them.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if there aren't any IDs provided.
            if (Input.Ids == null || !Input.Ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No or invalid IDs have been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Analyses
                    .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                    .Where(item => Input.Ids.Contains(item.Id))
                    .Include(item => item.AnalysisDatabases)
                        .ThenInclude(item => item.Database)
                            .ThenInclude(item => item.DatabaseType)
                    .Include(item => item.AnalysisDatabases)
                        .ThenInclude(item => item.Database)
                            .ThenInclude(item => item.DatabaseNodeFields)
                                .ThenInclude(item => item.DatabaseNodeFieldNodes)
                                    .ThenInclude(item => item.Node)
                    .Include(item => item.AnalysisDatabases)
                        .ThenInclude(item => item.Database)
                            .ThenInclude(item => item.DatabaseEdgeFields)
                                .ThenInclude(item => item.DatabaseEdgeFieldEdges)
                                    .ThenInclude(item => item.Edge)
                    .Include(item => item.AnalysisNodes)
                        .ThenInclude(item => item.Node)
                    .Include(item => item.AnalysisEdges)
                        .ThenInclude(item => item.Edge)
                            .ThenInclude(item => item.EdgeNodes)
                                .ThenInclude(item => item.Node)
                    .Include(item => item.AnalysisNetworks)
                        .ThenInclude(item => item.Network)
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No analyses have been found with the provided IDs, or you don't have access to them.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
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
                    foreach (var analysis in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"{analysis.Name}-{analysis.Id}.txt", CompressionLevel.Fastest).Open();
                        // Define the stream writer for the file.
                        using var streamWriter = new StreamWriter(stream);
                        // Get the default values.
                        var interactionType = analysis.AnalysisDatabases
                            .FirstOrDefault()?.Database.DatabaseType.Name.ToLower();
                        // Get the required data.
                        var data = string.Join("\n", analysis.AnalysisEdges.Select(item => item.Edge).Select(item =>
                        {
                            var sourceNode = item.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source)?.Node;
                            var targetNode = item.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target)?.Node;
                            return $"{sourceNode?.Name}\t{interactionType}\t{targetNode.Name}";
                        }));
                        // Write the data corresponding to the file.
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
                    foreach (var analysis in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"{analysis.Name}-{analysis.Id}.json", CompressionLevel.Fastest).Open();
                        // Define the data to be serialized to the file.
                        var data = new
                        {
                            Id = analysis.Id,
                            Name = analysis.Name,
                            Description = analysis.Description,
                            Algorithm = analysis.Algorithm.ToString(),
                            Nodes = analysis.AnalysisNodes
                                .Select(item => item.Node)
                                .Select(item => new
                                {
                                    Id = item.Id,
                                    Name = item.Name,
                                    Description = item.Description,
                                    Values = item.DatabaseNodeFieldNodes
                                        .Where(item1 => item1.DatabaseNodeField.Database.IsPublic || item1.DatabaseNodeField.Database.DatabaseUsers.Any(item2 => item2.User == user))
                                        .Select(item1 => new
                                        {
                                            DatabaseId = item1.DatabaseNodeField.Database.Id,
                                            DatabaseName = item1.DatabaseNodeField.Database.Name,
                                            DatabaseFieldId = item1.DatabaseNodeField.Id,
                                            DatabaseFieldName = item1.DatabaseNodeField.Name,
                                            Value = item1.Value
                                        })
                                }),
                            Edges = analysis.AnalysisEdges
                                .Select(item => item.Edge)
                                .Select(item => new
                                {
                                    Id = item.Id,
                                    Description = item.Description,
                                    Nodes = item.EdgeNodes
                                        .Select(item1 => new
                                        {
                                            NodeId = item1.Node.Id,
                                            NodeName = item1.Node.Name,
                                            Type = item1.Type.ToString()
                                        }),
                                    Values = item.DatabaseEdgeFieldEdges
                                        .Where(item1 => item1.DatabaseEdgeField.Database.IsPublic || item1.DatabaseEdgeField.Database.DatabaseUsers.Any(item2 => item2.User == user))
                                        .Select(item1 => new
                                        {
                                            DatabaseId = item1.DatabaseEdgeField.Database.Id,
                                            DatabaseName = item1.DatabaseEdgeField.Database.Name,
                                            DatabaseFieldId = item1.DatabaseEdgeField.Id,
                                            DatabaseFieldName = item1.DatabaseEdgeField.Name,
                                            Value = item1.Value
                                        })
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
                        IgnoreNullValues = true
                    };
                    // Go over each of the analyses to download.
                    foreach (var analysis in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"{analysis.Name}-{analysis.Id}.json", CompressionLevel.Fastest).Open();
                        // Write the data corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, analysis.GetCytoscapeViewModel(_linkGenerator), jsonSerializerOptions);
                    }
                }
                else if (Input.FileFormat == "Excel")
                {
                    // Go over each of the analyses to download.
                    foreach (var analysis in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"{analysis.Name}-{analysis.Id}.xlsx", CompressionLevel.Fastest).Open();
                        // Get the required data.
                        var databases = analysis.AnalysisDatabases
                            .Select(item => item.Database)
                            .Where(item1 => item1.IsPublic || item1.DatabaseUsers.Any(item2 => item2.User == user));
                        var databaseNodeFields = databases
                            .Select(item => item.DatabaseNodeFields)
                            .SelectMany(item => item);
                        var databaseEdgeFields = databases
                            .Select(item => item.DatabaseEdgeFields)
                            .SelectMany(item => item);
                        // Define the rows in the first sheet.
                        var worksheet1Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Date started", "Date ended", "Name", "Description", "Algorithm" },
                            new List<string> { analysis.Id, analysis.DateTimeStarted?.ToString(), analysis.DateTimeEnded?.ToString(), analysis.Name, analysis.Description, analysis.Algorithm.ToString() }
                        };
                        // Define the rows in the second sheet.
                        var worksheet2Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Name", "Type" }
                                .Concat(databaseNodeFields
                                    .Select(item => $"({item.Database.Name}) {item.Name}"))
                                .ToList()
                        }
                        .Concat(analysis.AnalysisNodes
                            .Select(item =>
                                new List<string> { item.Node.Id, item.Node.Name, item.Type.ToString() }
                                    .Concat(databaseNodeFields
                                        .Select(item1 => item1.DatabaseNodeFieldNodes.FirstOrDefault(item2 => item2.Node == item.Node)?.Value))
                                    .ToList()))
                        .ToList();
                        // Define the rows in the third sheet.
                        var worksheet3Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Source node ID", "Source node name", "Target node ID", "Target node name" }
                                .Concat(databaseEdgeFields
                                    .Select(item => item.Name))
                                .ToList()
                        }
                        .Concat(analysis.AnalysisEdges
                            .Select(item =>
                            {
                                var sourceNode = item.Edge.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source)?.Node;
                                var targetNode = item.Edge.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target)?.Node;
                                return new List<string> { item.Edge.Id, sourceNode?.Id, sourceNode?.Name, targetNode?.Id, targetNode?.Name }
                                    .Concat(databaseEdgeFields
                                        .Select(item1 => item1.DatabaseEdgeFieldEdges.FirstOrDefault(item2 => item2.Edge == item.Edge)?.Value))
                                    .ToList();
                            }))
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
                FileDownloadName = $"NetControl4BioMed-Analyses.zip"
            };
        }
    }
}
