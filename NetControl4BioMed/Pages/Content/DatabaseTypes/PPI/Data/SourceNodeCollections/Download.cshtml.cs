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
using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Services;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.PPI.Data.SourceNodeCollections
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
            [RegularExpression("Text|Json|Excel", ErrorMessage = "The value is not valid.")]
            public string FileFormat { get; set; }

            public string ReCaptchaToken { get; set; }

            public IEnumerable<string> Ids { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public IEnumerable<NodeCollection> Items { get; set; }
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
                return RedirectToPage("/Content/DatabaseTypes/PPI/Data/SourceNodeCollections/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.NodeCollections
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI"))
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
                    .Where(item => item.NodeCollectionNodes.Any(item1 => item1.Node.DatabaseNodes.Any(item2 => item2.Database.IsPublic || item2.Database.DatabaseUsers.Any(item3 => item3.User == user))))
                    .Where(item => item.NodeCollectionTypes.Any(item1 => item1.Type == NetControl4BioMed.Data.Enumerations.NodeCollectionType.Source))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No source protein collections have been found with the provided IDs, or you don't have access to them.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Data/SourceNodeCollections/Index");
            }
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
                return RedirectToPage("/Content/DatabaseTypes/PPI/Data/SourceNodeCollections/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.NodeCollections
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI"))
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
                    .Where(item => item.NodeCollectionNodes.Any(item1 => item1.Node.DatabaseNodes.Any(item2 => item2.Database.IsPublic || item2.Database.DatabaseUsers.Any(item3 => item3.User == user))))
                    .Where(item => item.NodeCollectionTypes.Any(item1 => item1.Type == NetControl4BioMed.Data.Enumerations.NodeCollectionType.Source))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No source protein collections have been found with the provided IDs, or you don't have access to them.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Data/SourceNodeCollections/Index");
            }
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
                    // Go over each of the node collection to download.
                    foreach (var nodeCollection in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"SourceProteinCollection-{nodeCollection.Name.Replace(" ", "-")}-{nodeCollection.Id}.txt", CompressionLevel.Fastest).Open();
                        // Define the stream writer for the file.
                        using var streamWriter = new StreamWriter(stream);
                        // Get the required data.
                        var data = string.Join("\n", _context.NodeCollectionNodes
                            .Where(item => item.NodeCollection == nodeCollection)
                            .Select(item => item.Node.Name));
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
                    // Go over each of the node collections to download.
                    foreach (var nodeCollection in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"SourceProteinCollection-{nodeCollection.Name.Replace(" ", "-")}-{nodeCollection.Id}.json", CompressionLevel.Fastest).Open();
                        // Get the required data.
                        var data = new
                        {
                            Name = nodeCollection.Name,
                            Description = nodeCollection.Description,
                            NodeCollectionNodes = _context.NodeCollectionNodes
                                .Where(item => item.NodeCollection == nodeCollection)
                                .Select(item => new
                                {
                                    Id = item.Node.Id,
                                    Name = item.Node.Name
                                })
                        };
                        // Write the data corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
                    }
                }
                else if (Input.FileFormat == "Excel")
                {
                    // Go over each of the node collections to download.
                    foreach (var nodeCollection in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"SourceProteinCollection-{nodeCollection.Name.Replace(" ", "-")}-{nodeCollection.Id}.xlsx", CompressionLevel.Fastest).Open();
                        // Get the required data.
                        var databases = _context.NodeCollectionDatabases
                            .Where(item => item.NodeCollection == nodeCollection)
                            .Select(item => item.Database)
                            .Where(item1 => item1.IsPublic || item1.DatabaseUsers.Any(item2 => item2.User == user));
                        var databaseNodeFields = _context.DatabaseNodeFields
                            .Where(item => databases.Contains(item.Database))
                            .Select(item => new
                            {
                                Id = item.Id,
                                Name = item.Name,
                                DatabaseName = item.Database.Name
                            })
                            .ToList();
                        // Define the rows in the first sheet.
                        var worksheet1Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Name", "Description" },
                            new List<string> { nodeCollection.Id, nodeCollection.Name, nodeCollection.Description }
                        };
                        // Define the rows in the second sheet.
                        var worksheet2Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Name" }
                                .Concat(databaseNodeFields
                                    .Select(item => $"{item.Name} ({item.DatabaseName})")
                                    .ToList())
                                .ToList()
                        }
                        .Concat(_context.NodeCollectionNodes
                            .Where(item => item.NodeCollection == nodeCollection)
                            .Select(item => item.Node)
                            .Select(item => new
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Description = item.Description,
                                Values = item.DatabaseNodeFieldNodes
                                    .Select(item1 => new
                                    {
                                        DatabaseNodeFieldId = item1.DatabaseNodeField.Id,
                                        Value = item1.Value
                                    })
                            })
                            .AsEnumerable()
                            .Select(item => new List<string> { item.Id, item.Name }
                                .Concat(databaseNodeFields
                                    .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseNodeFieldId == item1.Id))
                                    .Select(item1 => item1 == null ? string.Empty : item1.Value)
                                    .ToList())))
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
                        var worksheet2 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet2Part), SheetId = 2, Name = "Proteins" };
                        worksheet2Part.Worksheet = new Worksheet(worksheet2Data);
                        worksheet2Data.Append(worksheet2Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
                        worksheets.Append(worksheet2);
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
                FileDownloadName = $"NetControl4BioMed-Networks-{DateTime.UtcNow:yyyyMMdd}.zip"
            };
        }
    }
}
