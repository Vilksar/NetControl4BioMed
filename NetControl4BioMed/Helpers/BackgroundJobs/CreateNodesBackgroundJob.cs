using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.BackgroundJobs
{
    /// <summary>
    /// Implements a background job to create nodes in the database.
    /// </summary>
    public class CreateNodesBackgroundJob : BaseBackgroundJob
    {
        /// <summary>
        /// Gets or sets the items to be created.
        /// </summary>
        public IEnumerable<DataUpdateNodeViewModel> Items { get; set; }

        /// <summary>
        /// Runs the current job.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public override void Run(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null || !Items.Any())
            {
                // Throw an exception.
                throw new ArgumentException("No valid items could be found with the provided data.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / _batchSize);
            // Go over each batch.
            for (var index = 0; index < count; index++)
            {
                // Check if the cancellation was requested.
                if (token.IsCancellationRequested)
                {
                    // Break.
                    break;
                }
                // Get the items in the current batch.
                var batchItems = Items.Skip(index * _batchSize).Take(_batchSize);
                // Create a new scope.
                using var scope = serviceProvider.CreateScope();
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the manually provided IDs of all the items that are to be created.
                var itemNodeIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                // Check if any of the manually provided IDs are repeating in the list.
                if (itemNodeIds.Distinct().Count() != itemNodeIds.Count())
                {
                    // Throw an exception.
                    throw new ArgumentException("One or more of the manually provided IDs are duplicated.");
                }
                // Get the valid manually provided IDs, that do not appear in the database.
                var validItemNodeIds = itemNodeIds
                    .Except(context.Nodes
                        .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                        .Where(item => itemNodeIds.Contains(item.Id))
                        .Select(item => item.Id));
                // Get the IDs of all of the node fields that are to be updated.
                var itemNodeFieldIds = batchItems
                    .Select(item => item.Fields)
                    .SelectMany(item => item)
                    .Where(item => !string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value))
                    .Select(item => item.Key)
                    .Distinct();
                // Get the node fields that are to be updated.
                var nodeFields = context.DatabaseNodeFields
                    .Where(item => item.Database.DatabaseType.Name != "Generic")
                    .Where(item => itemNodeFieldIds.Contains(item.Id))
                    .Include(item => item.Database)
                    .AsEnumerable();
                // Check if there weren't any node fields found.
                if (nodeFields == null || !nodeFields.Any())
                {
                    // Throw an exception.
                    throw new ArgumentException("No database node fields could be found in the database with the provided IDs.");
                }
                // Get the valid database node field IDs.
                var validItemNodeFieldIds = nodeFields
                    .Select(item => item.Id);
                // Save the nodes to add.
                var nodes = new List<Node>();
                // Go over each of the items.
                foreach (var item in batchItems)
                {
                    // Check if the ID of the current item is valid.
                    if (!string.IsNullOrEmpty(item.Id) && !validItemNodeIds.Contains(item.Id))
                    {
                        // Continue.
                        continue;
                    }
                    // Get the valid item fields and the node field nodes to add.
                    var nodeFieldNodes = item.Fields
                        .Select(item1 => (item1.Key, item1.Value))
                        .Distinct()
                        .Where(item1 => validItemNodeFieldIds.Contains(item1.Key))
                        .Select(item1 => new DatabaseNodeFieldNode { DatabaseNodeFieldId = item1.Key, DatabaseNodeField = nodeFields.FirstOrDefault(item2 => item1.Key == item2.Id), Value = item1.Value })
                        .Where(item1 => item1.DatabaseNodeField != null);
                    // Check if there weren't any node fields found.
                    if (nodeFieldNodes == null || !nodeFieldNodes.Any() || !nodeFieldNodes.Any(item1 => item1.DatabaseNodeField.IsSearchable))
                    {
                        // Continue.
                        continue;
                    }
                    // Define the new node.
                    var node = new Node
                    {
                        Name = nodeFieldNodes.First(item1 => item1.DatabaseNodeField.IsSearchable).Value,
                        Description = item.Description,
                        DateTimeCreated = DateTime.Now,
                        DatabaseNodeFieldNodes = nodeFieldNodes.ToList(),
                        DatabaseNodes = nodeFieldNodes
                            .Select(item1 => item1.DatabaseNodeField.Database)
                            .Distinct()
                            .Select(item1 => new DatabaseNode { DatabaseId = item1.Id, Database = item1 })
                            .ToList()
                    };
                    // Check if there is any ID provided.
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        // Assign it to the node.
                        node.Id = item.Id;
                    }
                    // Add the new node to the list.
                    nodes.Add(node);
                }
                // Try to create the items.
                try
                {
                    // Create the items.
                    Create(nodes, context, token);
                }
                catch (Exception exception)
                {
                    // Throw an exception.
                    throw exception;
                }
            }
        }
    }
}
