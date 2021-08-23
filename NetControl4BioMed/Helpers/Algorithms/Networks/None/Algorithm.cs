using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Networks.None
{
    /// <summary>
    /// Defines the algorithm.
    /// </summary>
    public static class Algorithm
    {
        /// <summary>
        /// Runs the algorithm on the network with the provided details, using the given parameters.
        /// </summary>
        /// <param name="networkId">The ID of the network on which to run the algorithm.</param>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        /// <returns></returns>
        public static async Task Run(string networkId, IServiceProvider serviceProvider, CancellationToken token)
        {
            // Define the required data.
            var data = new List<NetworkInteractionInputModel>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Reload the network.
                var network = context.Networks
                    .FirstOrDefault(item => item.Id == networkId);
                // Check if there was no item found.
                if (network == null)
                {
                    // Return.
                    return;
                }
                // Try to deserialize the data.
                if (!network.Data.TryDeserializeJsonObject<List<NetworkInteractionInputModel>>(out data) || data == null)
                {
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog("The seed data corresponding to the network could not be deserialized.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                    // End the function.
                    return;
                }
            }
            // Get the seed interactions from the data.
            var seedInteractions = data
                .Where(item => item.Interaction != null)
                .Select(item => item.Interaction)
                .Where(item => item.InteractionProteins != null)
                .Select(item => (item.InteractionProteins.FirstOrDefault(item1 => item1.Type == "Source"), item.InteractionProteins.FirstOrDefault(item1 => item1.Type == "Target")))
                .Where(item => item.Item1 != null && item.Item2 != null)
                .Select(item => (item.Item1.Protein, item.Item2.Protein))
                .Where(item => item.Item1 != null && item.Item2 != null)
                .Select(item => (item.Item1.Id, item.Item2.Id))
                .Where(item => !string.IsNullOrEmpty(item.Item1) && !string.IsNullOrEmpty(item.Item2))
                .Distinct();
            // Check if there haven't been any interactions found.
            if (seedInteractions == null || !seedInteractions.Any())
            {
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Reload the network.
                    var network = context.Networks
                        .FirstOrDefault(item => item.Id == networkId);
                    // Check if there was no item found.
                    if (network == null)
                    {
                        // Return.
                        return;
                    }
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog($"The seed data corresponding to the network does not contain any valid interactions.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                }
                // End the function.
                return;
            }
            // Get the seed proteins from the seed interactions.
            var seedProteins = seedInteractions
                .Select(item => item.Item1)
                .Concat(seedInteractions.Select(item => item.Item2))
                .Distinct();
            // Define the related entities.
            var networkProteins = new List<NetworkProtein>();
            var networkInteractions = new List<NetworkInteraction>();
            // Define the related entities.
            networkProteins = seedProteins
                .Select(item => new NetworkProtein
                {
                    Protein = new Protein
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = item,
                        Description = null
                    },
                    Type = NetworkProteinType.None
                })
                .ToList();
            networkInteractions = seedInteractions
                .Select(item => new NetworkInteraction
                {
                    Interaction = new Interaction
                    {
                        DateTimeCreated = DateTime.UtcNow,
                        Name = $"{item.Item1} - {item.Item2}",
                        Description = null,
                        InteractionProteins = new List<InteractionProtein>
                        {
                            new InteractionProtein
                            {
                                Protein = networkProteins
                                    .FirstOrDefault(item1 => item1.Protein.Name == item.Item1)?.Protein,
                                Type = InteractionProteinType.Source
                            },
                            new InteractionProtein
                            {
                                Protein = networkProteins
                                    .FirstOrDefault(item1 => item1.Protein.Name == item.Item2)?.Protein,
                                Type = InteractionProteinType.Target
                            }
                        }
                        .Where(item1 => item1.Protein != null)
                        .ToList()
                    }
                })
                .ToList();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Reload the network.
                var network = context.Networks
                    .FirstOrDefault(item => item.Id == networkId);
                // Check if there was no item found.
                if (network == null)
                {
                    // Return.
                    return;
                }
                // Update the network.
                network.NetworkProteins = networkProteins;
                network.NetworkInteractions = networkInteractions;
                // Edit the network.
                await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
            }
        }
    }
}
