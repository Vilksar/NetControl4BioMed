using DocumentFormat.OpenXml.InkML;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Implements an administration task manager.
    /// </summary>
    public class AdministrationTaskManager : IAdministrationTaskManager
    {
        /// <summary>
        /// Represents the application service provider.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        public AdministrationTaskManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates nodes in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task CreateNodesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<NodesTask>(backgroundTask);
            // Run the task.
            await task.CreateAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Creates edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task CreateEdgesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<EdgesTask>(backgroundTask);
            // Run the task.
            await task.CreateAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Creates node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task CreateNodeCollectionsAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<NodeCollectionsTask>(backgroundTask);
            // Run the task.
            await task.CreateAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Edits nodes in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task EditNodesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<NodesTask>(backgroundTask);
            // Run the task.
            await task.EditAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Edits edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task EditEdgesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<EdgesTask>(backgroundTask);
            // Run the task.
            await task.EditAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Edits node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task EditNodeCollectionsAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<NodeCollectionsTask>(backgroundTask);
            // Run the task.
            await task.EditAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteUsersAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<UsersTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteRolesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<RolesTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes user roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteUserRolesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<UserRolesTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes database users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteDatabaseUsersAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<DatabaseUsersTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes database user invitations from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteDatabaseUserInvitationsAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<DatabaseUserInvitationsTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes database types from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteDatabaseTypesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<DatabaseTypesTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes databases from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteDatabasesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<DatabasesTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes database node fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteDatabaseNodeFieldsAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<DatabaseNodeFieldsTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes database edge fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteDatabaseEdgeFieldsAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<DatabaseEdgeFieldsTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes nodes from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteNodesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<NodesTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes edges from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteEdgesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<EdgesTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes node collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteNodeCollectionsAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<NodeCollectionsTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes samples from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteSamplesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<SamplesTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteNetworksAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<NetworksTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAnalysesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<AnalysesTask>(backgroundTask);
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Stops analyses in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task StopAnalysesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetTask<AnalysesTask>(backgroundTask);
            // Run the task.
            await task.StopAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes all nodes from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAllNodesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<NodesTask>(backgroundTask);
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get all of the item IDs.
            task.Items = context.Nodes
                .Where(item => !item.DatabaseNodes.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Select(item => item.Id)
                .ToList()
                .Select(item => new InputModels.NodeInputModel
                {
                    Id = item
                });
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes all edges from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAllEdgesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<EdgesTask>(backgroundTask);
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get all of the item IDs.
            task.Items = context.Edges
                .Where(item => !item.DatabaseEdges.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                .Select(item => item.Id)
                .ToList()
                .Select(item => new InputModels.EdgeInputModel
                {
                    Id = item
                });
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes all node collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAllNodeCollectionsAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<NodeCollectionsTask>(backgroundTask);
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get all of the item IDs.
            task.Items = context.NodeCollections
                .Select(item => item.Id)
                .ToList()
                .Select(item => new InputModels.NodeCollectionInputModel
                {
                    Id = item
                });
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes all networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAllNetworksAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<NetworksTask>(backgroundTask);
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get all of the item IDs.
            task.Items = context.Networks
                .Select(item => item.Id)
                .ToList()
                .Select(item => new InputModels.NetworkInputModel
                {
                    Id = item
                });
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes all analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAllAnalysesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<AnalysesTask>(backgroundTask);
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get all of the item IDs.
            task.Items = context.Analyses
                .Select(item => item.Id)
                .ToList()
                .Select(item => new InputModels.AnalysisInputModel
                {
                    Id = item
                });
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Deletes all samples from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAllSamplesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<SamplesTask>(backgroundTask);
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Get all of the item IDs.
            task.Items = context.Samples
                .Select(item => item.Id)
                .ToList()
                .Select(item => new InputModels.SampleInputModel
                {
                    Id = item
                });
            // Run the task.
            await task.DeleteAsync(_serviceProvider, token);
            // Complete the task.
            await DeleteBackgroundTaskAsync(backgroundTask);
        }

        /// <summary>
        /// Gets from the database the background task with the provided ID.
        /// </summary>
        /// <param name="id">The internal ID of the background task.</param>
        /// <returns>The background task corresponding to the provided ID.</returns>
        private BackgroundTask GetBackgroundTask(string id)
        {
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Try to get the background task with the provided ID.
            var backgroundTask = context.BackgroundTasks
                .Where(item => item.Id == id)
                .FirstOrDefault();
            // Check if there was any task found.
            if (backgroundTask == null)
            {
                // Throw an exception.
                throw new ArgumentException("No task could be found with the provided ID.");
            }
            // Return the task.
            return backgroundTask;
        }

        /// <summary>
        /// Gets the task corresponding to the provided background task.
        /// </summary>
        /// <typeparam name="T">The type of the corresponding task.</typeparam>
        /// <param name="backgroundTask">The current background task.</param>
        /// <returns>The task corresponding to the provided background task.</returns>
        private T GetTask<T>(BackgroundTask backgroundTask)
        {
            // Try to deserialize the task.
            if (!backgroundTask.Data.TryDeserializeJsonObject<T>(out var task) || task == null)
            {
                // Throw an exception.
                throw new ArgumentException("The data of the task could not be deserialized.");
            }
            // Return the task.
            return task;
        }

        /// <summary>
        /// Deletes the provided background task.
        /// </summary>
        /// <param name="backgroundTask">The current background task.</param>
        private async Task DeleteBackgroundTaskAsync(BackgroundTask backgroundTask)
        {
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Mark the task for deletion.
            context.BackgroundTasks.Remove(backgroundTask);
            // Save the changes.
            await context.SaveChangesAsync();
        }
    }
}
