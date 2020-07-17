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
using NetControl4BioMed.Helpers.Services;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Content.Created.Networks
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
            public IEnumerable<Network> Items { get; set; }
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
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Networks
                    .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
                    .Where(item => ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No networks have been found with the provided IDs, or you don't have access to them.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
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
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Items = _context.Networks
                    .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
                    .Where(item => Input.Ids.Contains(item.Id))
            };
            // Check if there weren't any items found.
            if (View.Items == null || !View.Items.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No networks have been found with the provided IDs, or you don't have access to them.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
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
                    // Go over each of the networks to download.
                    foreach (var network in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Network-{network.Name.Replace(" ", "-")}-{network.Id}.txt", CompressionLevel.Fastest).Open();
                        // Define the stream writer for the file.
                        using var streamWriter = new StreamWriter(stream);
                        // Get the default values.
                        var interactionType = _context.NetworkDatabases
                            .Where(item => item.Network == network)
                            .Select(item => item.Database.DatabaseType.Name.ToLower())
                            .FirstOrDefault() ?? "unknown";
                        // Get the required data.
                        var data = string.Join("\n", _context.NetworkEdges
                            .Where(item => item.Network == network)
                            .Select(item => item.Edge)
                            .Select(item => new
                            {
                                SourceNodeName = item.EdgeNodes
                                    .Where(item1 => item1.Type == EdgeNodeType.Source)
                                    .Select(item1 => item1.Node)
                                    .Where(item1 => item1 != null)
                                    .Select(item1 => item1.Name)
                                    .FirstOrDefault(),
                                TargetNodeName = item.EdgeNodes
                                    .Where(item1 => item1.Type == EdgeNodeType.Target)
                                    .Select(item1 => item1.Node)
                                    .Where(item1 => item1 != null)
                                    .Select(item1 => item1.Name)
                                    .FirstOrDefault()
                            })
                            .Where(item => !string.IsNullOrEmpty(item.SourceNodeName) && !string.IsNullOrEmpty(item.TargetNodeName))
                            .Select(item => $"{item.SourceNodeName}\t{interactionType}\t{item.TargetNodeName}"));
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
                    // Go over each of the networks to download.
                    foreach (var network in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Network-{network.Name.Replace(" ", "-")}-{network.Id}.json", CompressionLevel.Fastest).Open();
                        // Get the required data.
                        var data = new
                        {
                            Id = network.Id,
                            Name = network.Name,
                            Description = network.Description,
                            Algorithm = network.Algorithm.ToString(),
                            Nodes = _context.NetworkNodes
                                .Where(item => item.Network == network)
                                .Where(item => item.Type == NetworkNodeType.None)
                                .Select(item => item.Node)
                                .Select(item => new
                                {
                                    Id = item.Id,
                                    Name = item.Name,
                                    Description = item.Description,
                                    Values = item.DatabaseNodeFieldNodes
                                        .Select(item1 => new
                                        {
                                            DatabaseId = item1.DatabaseNodeField.Database.Id,
                                            DatabaseName = item1.DatabaseNodeField.Database.Name,
                                            DatabaseNodeFieldId = item1.DatabaseNodeField.Id,
                                            DatabaseNodeFieldName = item1.DatabaseNodeField.Name,
                                            Value = item1.Value
                                        })
                                }),
                            Edges = _context.NetworkEdges
                                .Where(item => item.Network == network)
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
                                        .Select(item1 => new
                                        {
                                            DatabaseId = item1.DatabaseEdgeField.Database.Id,
                                            DatabaseName = item1.DatabaseEdgeField.Database.Name,
                                            DatabaseEdgeFieldId = item1.DatabaseEdgeField.Id,
                                            DatabaseEdgeFieldName = item1.DatabaseEdgeField.Name,
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
                        IgnoreNullValues = true,
                        WriteIndented = true
                    };
                    // Go over each of the networks to download.
                    foreach (var network in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Network-{network.Name.Replace(" ", "-")}-{network.Id}.json", CompressionLevel.Fastest).Open();
                        // Write the data corresponding to the file.
                        await JsonSerializer.SerializeAsync(stream, network.GetCytoscapeViewModel(_linkGenerator, _context), jsonSerializerOptions);
                    }
                }
                else if (Input.FileFormat == "Excel")
                {
                    // Go over each of the networks to download.
                    foreach (var network in View.Items)
                    {
                        // Create a new entry in the archive and open it.
                        using var stream = archive.CreateEntry($"Network-{network.Name.Replace(" ", "-")}-{network.Id}.xlsx", CompressionLevel.Fastest).Open();
                        // Get the required data.
                        var databases = _context.NetworkDatabases
                            .Where(item => item.Network == network)
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
                        var databaseEdgeFields = _context.DatabaseEdgeFields
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
                            new List<string> { "Internal ID", "Date", "Name", "Description", "Algorithm" },
                            new List<string> { network.Id, network.DateTimeCreated.ToString(), network.Name, network.Description, network.Algorithm.ToString() }
                        };
                        // Define the rows in the second sheet.
                        var worksheet2Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Name", "Type" }
                                .Concat(databaseNodeFields
                                    .Select(item => $"{item.Name} ({item.DatabaseName})")
                                    .ToList())
                                .ToList()
                        }
                        .Concat(_context.NetworkNodes
                            .Where(item => item.Network == network)
                            .Where(item => item.Type == NetworkNodeType.None)
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
                                    }),
                                Types = item.NetworkNodes
                                    .Where(item1 => item1.Network == network)
                                    .Select(item => item.Type.ToString().ToLower())
                            })
                            .AsEnumerable()
                            .Select(item => new List<string> { item.Id, item.Name, string.Join(", ", item.Types) }
                                .Concat(databaseNodeFields
                                    .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseNodeFieldId == item1.Id))
                                    .Select(item1 => item1 == null ? string.Empty : item1.Value)
                                    .ToList())))
                        .ToList();
                        // Define the rows in the third sheet.
                        var worksheet3Rows = new List<List<string>>
                        {
                            new List<string> { "Internal ID", "Source node ID", "Source node name", "Target node ID", "Target node name" }
                                .Concat(databaseEdgeFields
                                    .Select(item => $"{item.Name} ({item.DatabaseName})")
                                    .ToList())
                                .ToList()
                        }
                        .Concat(_context.NetworkEdges
                            .Where(item => item.Network == network)
                            .Select(item => item.Edge)
                            .Select(item => new
                            {
                                Id = item.Id,
                                SourceNode = item.EdgeNodes
                                    .Where(item1 => item1.Type == EdgeNodeType.Source)
                                    .Select(item1 => item1.Node)
                                    .FirstOrDefault(),
                                TargetNode = item.EdgeNodes
                                    .Where(item1 => item1.Type == EdgeNodeType.Target)
                                    .Select(item1 => item1.Node)
                                    .FirstOrDefault(),
                                Values = item.DatabaseEdgeFieldEdges
                                    .Select(item1 => new
                                    {
                                        DatabaseEdgeFieldId = item1.DatabaseEdgeField.Id,
                                        Value = item1.Value
                                    })
                            })
                            .Where(item => item.SourceNode != null && item.TargetNode != null)
                            .AsEnumerable()
                            .Select(item => new List<string> { item.Id, item.SourceNode.Id, item.SourceNode.Name, item.TargetNode.Id, item.TargetNode.Name }
                                .Concat(databaseEdgeFields
                                    .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseEdgeFieldId == item1.Id))
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
                FileDownloadName = $"NetControl4BioMed-Networks.zip"
            };
        }
    }
}
