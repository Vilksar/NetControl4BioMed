using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Interfaces;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for the protein collections.
    /// </summary>
    public static class ProteinCollectionExtensions
    {
        /// <summary>
        /// Deletes the related entities of the corresponding protein collections.
        /// </summary>
        /// <typeparam name="T">The type of the related entities.</typeparam>
        /// <param name="itemIds">The items whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteRelatedEntitiesAsync<T>(IEnumerable<string> itemIds, IServiceProvider serviceProvider, CancellationToken token) where T : class, IProteinCollectionDependent
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the corresponding database set.
                var set = context.Set<T>();
                // Get the items in the current batch.
                entityCount = set
                    .Where(item => itemIds.Contains(item.ProteinCollection.Id))
                    .Count();
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)entityCount / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (int index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Define the batch items.
                var batchItems = new List<T>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the corresponding database set.
                    var set = context.Set<T>();
                    // Get the items in the current batch.
                    batchItems = set
                        .Where(item => itemIds.Contains(item.ProteinCollection.Id))
                        .Take(ApplicationDbContext.BatchSize)
                        .ToList();
                    // Check if there were no items found.
                    if (batchItems == null || !batchItems.Any())
                    {
                        // Continue.
                        continue;
                    }
                }
                // Delete the items.
                await IEnumerableExtensions.DeleteAsync(batchItems, serviceProvider, token);
            }
        }

        /// <summary>
        /// Deletes the dependent analyses of the corresponding protein collections.
        /// </summary>
        /// <param name="proteinCollectionIds">The protein collections whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteDependentAnalysesAsync(IEnumerable<string> proteinCollectionIds, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                entityCount = context.AnalysisProteinCollections
                    .Where(item => proteinCollectionIds.Contains(item.ProteinCollection.Id))
                    .Select(item => item.Analysis)
                    .Distinct()
                    .Count();
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)entityCount / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (int index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Define the batch items.
                var batchItemInputs = new List<AnalysisInputModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    batchItemInputs = context.AnalysisProteinCollections
                        .Where(item => proteinCollectionIds.Contains(item.ProteinCollection.Id))
                        .Select(item => item.Analysis)
                        .Distinct()
                        .Select(item => new AnalysisInputModel
                        {
                            Id = item.Id
                        })
                        .Take(ApplicationDbContext.BatchSize)
                        .ToList();
                    // Check if there were no items found.
                    if (batchItemInputs == null || !batchItemInputs.Any())
                    {
                        // Continue.
                        continue;
                    }
                }
                // Delete the items.
                await new AnalysesTask { Items = batchItemInputs }.DeleteAsync(serviceProvider, token);
            }
        }

        /// <summary>
        /// Deletes the dependent networks of the corresponding protein collections.
        /// </summary>
        /// <param name="proteinCollectionIds">The protein collections whose entities should be deleted.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public static async Task DeleteDependentNetworksAsync(IEnumerable<string> proteinCollectionIds, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define a variable to store the total number of entities.
            var entityCount = 0;
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the items in the current batch.
                entityCount = context.NetworkProteinCollections
                    .Where(item => proteinCollectionIds.Contains(item.ProteinCollection.Id))
                    .Select(item => item.Network)
                    .Distinct()
                    .Count();
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)entityCount / ApplicationDbContext.BatchSize);
            // Go over each batch.
            for (int index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Define the batch items.
                var batchItemInputs = new List<NetworkInputModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items in the current batch.
                    batchItemInputs = context.NetworkProteinCollections
                        .Where(item => proteinCollectionIds.Contains(item.ProteinCollection.Id))
                        .Select(item => item.Network)
                        .Distinct()
                        .Select(item => new NetworkInputModel
                        {
                            Id = item.Id
                        })
                        .Take(ApplicationDbContext.BatchSize)
                        .ToList();
                    // Check if there were no items found.
                    if (batchItemInputs == null || !batchItemInputs.Any())
                    {
                        // Continue.
                        continue;
                    }
                }
                // Delete the items.
                await new NetworksTask { Items = batchItemInputs }.DeleteAsync(serviceProvider, token);
            }
        }

        /// <summary>
        /// Gets the content of an overview text file to download corresponding to the provided protein collections.
        /// </summary>
        /// <param name="proteinCollectionIds">The IDs of the current protein collections.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="scheme">The HTTP context scheme.</param>
        /// <param name="host">The HTTP context host.</param>
        /// <returns></returns>
        public static async Task WriteToStreamOverviewTextFileContent(IEnumerable<string> proteinCollectionIds, Stream stream, IServiceProvider serviceProvider, string scheme, HostString host)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Use a new link generator instance.
            var linkGenerator = scope.ServiceProvider.GetRequiredService<LinkGenerator>();
            // Define the stream writer for the file.
            using var streamWriter = new StreamWriter(stream);
            // Get the required data.
            var data = string.Concat("The following collections have been downloaded:\n\n", string.Join("\n", context.ProteinCollections
                .Where(item => proteinCollectionIds.Contains(item.Id))
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name
                })
                .AsEnumerable()
                .Select(item => $"{item.Name} - {linkGenerator.GetUriByPage($"/AvailableData/Data/ProteinCollections/Details", handler: null, values: new { id = item.Id }, scheme: scheme, host: host)}")));
            // Write the data to the stream.
            await streamWriter.WriteAsync(data);
        }

        /// <summary>
        /// Gets the content of a text file to download corresponding to the provided control path.
        /// </summary>
        /// <param name="proteinCollection">The current protein collection.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the text file corresponding to the provided control path.</returns>
        public static async Task WriteToStreamTxtFileContent(this ProteinCollection proteinCollection, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Define the stream writer for the file.
            using var streamWriter = new StreamWriter(stream);
            // Get the data for the file.
            var data = string.Join("\n", context.ProteinCollectionProteins
                .Where(item => item.ProteinCollection == proteinCollection)
                .Select(item => item.Protein.Name));
            // Write the data to the stream.
            await streamWriter.WriteAsync(data);
        }

        /// <summary>
        /// Gets the content of a JSON file to download corresponding to the provided control path.
        /// </summary>
        /// <param name="proteinCollection">The current protein collection.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the JSON file corresponding to the provided control path.</returns>
        public static async Task WriteToStreamJsonFileContent(this ProteinCollection proteinCollection, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the required data.
            var data = new
            {
                Id = proteinCollection.Id,
                Name = proteinCollection.Name,
                Description = proteinCollection.Description,
                ProteinCollectionProteins = context.ProteinCollectionProteins
                    .Where(item => item.ProteinCollection == proteinCollection)
                    .Select(item => item.Protein.Name)
            };
            // Write the data corresponding to the file.
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { IgnoreNullValues = true });
        }

        /// <summary>
        /// Gets the content of an Excel file to download corresponding to the provided control path.
        /// </summary>
        /// <param name="proteinCollection">The current protein collection.</param>
        /// <param name="user">The current user.</param>
        /// <param name="stream">The stream to which to write to.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <returns>The content of the Excel file corresponding to the provided control path.</returns>
        public static async Task WriteToStreamXlsxFileContent(this ProteinCollection proteinCollection, User user, Stream stream, IServiceProvider serviceProvider)
        {
            // Use a new scope.
            using var scope = serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get the required data.
            var databaseProteinFields = context.DatabaseProteinFields
                .Where(item => item.Database.IsPublic || (user != null && item.Database.DatabaseUsers.Any(item => item.Email == user.Email)))
                .ToList();
            // Define the rows in the first sheet.
            var worksheet1Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    proteinCollection.Id
                },
                new List<string>
                {
                    "Name",
                    proteinCollection?.Name ?? string.Empty
                },
                new List<string>
                {
                    "Description",
                    proteinCollection?.Description ?? string.Empty
                }
            };
            // Define the rows in the second sheet.
            var worksheet2Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    "Name"
                }
            }.Concat(context.DatabaseProteinFields
                .Where(item => item.Database.IsPublic || (user != null && item.Database.DatabaseUsers.Any(item => item.Email == user.Email)))
                .Select(item => item.Database)
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name
                })
                .AsEnumerable()
                .Select(item => new List<string>
                {
                    item.Id,
                    item.Name
                })
                .ToList())
            .ToList();
            // Define the rows in the third sheet.
            var worksheet3Rows = new List<List<string>>
            {
                new List<string>
                {
                    "Internal ID",
                    "Name"
                }.Concat(databaseProteinFields
                    .Select(item => item.Name)
                    .ToList())
                .ToList()
            }
            .Concat(context.ProteinCollectionProteins
                .Where(item => item.ProteinCollection == proteinCollection)
                .Select(item => item.Protein)
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Values = item.DatabaseProteinFieldProteins
                        .Select(item1 => new
                        {
                            DatabaseProteinFieldId = item1.DatabaseProteinField.Id,
                            Value = item1.Value
                        })
                })
                .AsEnumerable()
                .Select(item => new List<string>
                    {
                        item.Id,
                        item.Name
                    }.Concat(databaseProteinFields
                        .Select(item1 => item.Values.FirstOrDefault(item2 => item2.DatabaseProteinFieldId == item1.Id))
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
            var worksheet2 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet2Part), SheetId = 2, Name = "Databases" };
            worksheet2Part.Worksheet = new Worksheet(worksheet2Data);
            worksheet2Data.Append(worksheet2Rows.Select(item => new Row(item.Select(item1 => new Cell { DataType = CellValues.String, CellValue = new CellValue(item1) }))));
            worksheets.Append(worksheet2);
            // Define the third worksheet.
            var worksheet3Part = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet3Data = new SheetData();
            var worksheet3 = new Sheet { Id = workbookPart.GetIdOfPart(worksheet3Part), SheetId = 3, Name = "Proteins" };
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
}
