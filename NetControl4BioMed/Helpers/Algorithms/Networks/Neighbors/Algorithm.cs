using Microsoft.EntityFrameworkCore;
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

namespace NetControl4BioMed.Helpers.Algorithms.Networks.Neighbors
{
    /// <summary>
    /// Defines the algorithm.
    /// </summary>
    public static class Algorithm
    {
        /// <summary>
        /// Represents the model of the required database data for an interaction.
        /// </summary>
        private class InteractionItemModel
        {
            /// <summary>
            /// Represents the ID of the interaction.
            /// </summary>
            public string InteractionId { get; set; }

            /// <summary>
            /// Represents the ID of the source protein of the interaction.
            /// </summary>
            public string SourceProteinId { get; set; }

            /// <summary>
            /// Represents the ID of the target protein of the interaction.
            /// </summary>
            public string TargetProteinId { get; set; }
        }

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
            var data = new List<NetworkProteinInputModel>();
            var proteinDatabaseIds = new List<string>();
            var interactionDatabaseIds = new List<string>();
            var seedProteinCollectionIds = new List<string>();
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
                if (!network.Data.TryDeserializeJsonObject<List<NetworkProteinInputModel>>(out data) || data == null)
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
                // Get the IDs of the required related data.
                proteinDatabaseIds = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkDatabaseType.Protein)
                    .Select(item => item.Database)
                    .Distinct()
                    .Select(item => item.Id)
                    .ToList();
                interactionDatabaseIds = context.NetworkDatabases
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkDatabaseType.Interaction)
                    .Select(item => item.Database)
                    .Distinct()
                    .Select(item => item.Id)
                    .ToList();
                seedProteinCollectionIds = context.NetworkProteinCollections
                    .Where(item => item.Network == network)
                    .Where(item => item.Type == NetworkProteinCollectionType.Seed)
                    .Select(item => item.ProteinCollection)
                    .Distinct()
                    .Select(item => item.Id)
                    .ToList();
            }
            // Get the protein identifiers from the data.
            var seedProteinIdentifiers = data
                .Where(item => item.Type == "Seed")
                .Select(item => item.Protein)
                .Where(item => item != null)
                .Select(item => item.Id)
                .Where(item => !string.IsNullOrEmpty(item))
                .Distinct();
            // Define the required data.
            var seedProteinIds = new List<string>();
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
                // Get the available proteins.
                var availableProteins = context.Proteins
                    .Where(item => item.DatabaseProteins.Any(item1 => proteinDatabaseIds.Contains(item1.Database.Id)));
                // Check if there haven't been any available proteins found.
                if (availableProteins == null || !availableProteins.Any())
                {
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog($"No available proteins could be found in the selected databases.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                    // End the function.
                    return;
                }
                // Get the available interactions.
                var availableInteractions = context.Interactions
                    .Where(item => item.DatabaseInteractions.Any(item1 => interactionDatabaseIds.Contains(item1.Database.Id)))
                    .Where(item => item.InteractionProteins.All(item1 => availableProteins.Contains(item1.Protein)));
                // Check if there haven't been any available interactions found.
                if (availableInteractions == null || !availableInteractions.Any())
                {
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog($"No available interactions could be found in the selected databases.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                    // End the function.
                    return;
                }
                // Get the seed proteins.
                var seedProteinsByIdentifier = availableProteins
                    .Where(item => seedProteinIdentifiers.Contains(item.Id) || item.DatabaseProteinFieldProteins.Any(item1 => item1.DatabaseProteinField.IsSearchable && seedProteinIdentifiers.Contains(item1.Value)));
                var seedProteinsByProteinCollection = availableProteins
                    .Where(item => item.ProteinCollectionProteins.Any(item1 => seedProteinCollectionIds.Contains(item1.ProteinCollection.Id)));
                seedProteinIds = seedProteinsByIdentifier
                    .Concat(seedProteinsByProteinCollection)
                    .Distinct()
                    .Select(item => item.Id)
                    .ToList();
                // Check if there haven't been any seed proteins found.
                if (seedProteinIds == null || !seedProteinIds.Any())
                {
                    // Update the status of the item.
                    network.Status = NetworkStatus.Error;
                    // Add a message to the log.
                    network.Log = network.AppendToLog($"No seed proteins could be found with the provided seed data.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                    // End the function.
                    return;
                }
            }
            // Define the list to store the interactions.
            var currentInteractionList = new List<InteractionItemModel>();
            // Use a new scope.
            using (var scope = serviceProvider.CreateScope())
            {
                // Use a new context instance.
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Get the available proteins.
                var availableProteins = context.Proteins
                    .Where(item => item.DatabaseProteins.Any(item1 => proteinDatabaseIds.Contains(item1.Database.Id)));
                // Define the interactions of the network.
                currentInteractionList = context.Interactions
                    .Where(item => item.DatabaseInteractions.Any(item1 => interactionDatabaseIds.Contains(item1.Database.Id)))
                    .Where(item => item.InteractionProteins.All(item1 => availableProteins.Contains(item1.Protein)))
                    .Where(item => item.InteractionProteins.Any(item1 => seedProteinIds.Contains(item1.Protein.Id)))
                    .Select(item => new InteractionItemModel
                    {
                        InteractionId = item.Id,
                        SourceProteinId = item.InteractionProteins
                            .Where(item => item.Type == InteractionProteinType.Source)
                            .Select(item => item.Protein.Id)
                            .FirstOrDefault(),
                        TargetProteinId = item.InteractionProteins
                            .Where(item => item.Type == InteractionProteinType.Target)
                            .Select(item => item.Protein.Id)
                            .FirstOrDefault()
                    })
                    .Where(item => !string.IsNullOrEmpty(item.InteractionId) && !string.IsNullOrEmpty(item.SourceProteinId) && !string.IsNullOrEmpty(item.TargetProteinId))
                    .ToList();
            }
            // Check if there haven't been any interactions found.
            if (currentInteractionList == null || !currentInteractionList.Any())
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
                    network.Log = network.AppendToLog($"No interactions could be found with the provided data using the provided algorithm.");
                    // Edit the network.
                    await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
                }
                // End the function.
                return;
            }
            // Define the interactions of the network.
            var interactionIds = currentInteractionList
                .Select(item => item.InteractionId)
                .Distinct()
                .ToList();
            // Get all of the proteins used by the found interactions.
            var proteinIds = currentInteractionList
                .Select(item => item.SourceProteinId)
                .Concat(currentInteractionList
                    .Select(item => item.TargetProteinId))
                .Distinct()
                .ToList();
            // Update the seed protein IDs.
            seedProteinIds = seedProteinIds
                .Intersect(proteinIds)
                .ToList();
            // Define the related entities.
            var networkProteins = proteinIds
                .Select(item => new NetworkProtein
                {
                    ProteinId = item,
                    Type = NetworkProteinType.None
                })
                .Concat(seedProteinIds
                    .Select(item => new NetworkProtein
                    {
                        ProteinId = item,
                        Type = NetworkProteinType.Seed
                    }))
                .ToList();
            var networkInteractions = interactionIds
                .Select(item => new NetworkInteraction
                {
                    InteractionId = item
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
                // Define the related entities.
                network.NetworkProteins = networkProteins;
                network.NetworkInteractions = networkInteractions;
                // Edit the network.
                await IEnumerableExtensions.EditAsync(network.Yield(), serviceProvider, token);
            }
        }
    }
}
