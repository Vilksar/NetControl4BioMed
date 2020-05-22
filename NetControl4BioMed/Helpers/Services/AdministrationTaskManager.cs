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
        /// Represents the application database context.
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        public AdministrationTaskManager(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            _serviceProvider = serviceProvider;
            _context = context;
        }

        /// <summary>
        /// Creates nodes in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void CreateNodes(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<NodesTask>(backgroundTask);
            // Run the task.
            _ = task.Create(_serviceProvider, token).ToList();
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Creates edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void CreateEdges(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<EdgesTask>(backgroundTask);
            // Run the task.
            _ = task.Create(_serviceProvider, token).ToList();
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Creates node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void CreateNodeCollections(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<NodeCollectionsTask>(backgroundTask);
            // Run the task.
            _ = task.Create(_serviceProvider, token).ToList();
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Edits nodes in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void EditNodes(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<NodesTask>(backgroundTask);
            // Run the task.
            _ = task.Edit(_serviceProvider, token).ToList();
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Edits edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void EditEdges(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<EdgesTask>(backgroundTask);
            // Run the task.
            _ = task.Edit(_serviceProvider, token).ToList();
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Edits node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void EditNodeCollections(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<NodeCollectionsTask>(backgroundTask);
            // Run the task.
            _ = task.Edit(_serviceProvider, token).ToList();
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteUsers(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<UsersTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteRoles(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<RolesTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes user roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteUserRoles(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<UserRolesTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes database users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteDatabaseUsers(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<DatabaseUsersTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes database user invitations from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteDatabaseUserInvitations(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<DatabaseUserInvitationsTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes database types from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteDatabaseTypes(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<DatabaseTypesTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes databases from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteDatabases(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<DatabasesTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes database node fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteDatabaseNodeFields(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<DatabaseNodeFieldsTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes database edge fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteDatabaseEdgeFields(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<DatabaseEdgeFieldsTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes nodes from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteNodes(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<NodesTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes edges from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteEdges(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<EdgesTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes node collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteNodeCollections(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<NodeCollectionsTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteNetworks(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<NetworksTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteAnalyses(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<AnalysesTask>(backgroundTask);
            // Run the task.
            task.Delete(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Stops analyses in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void StopAnalyses(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<AnalysesTask>(backgroundTask);
            // Run the task.
            task.Stop(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Gets from the database the background task with the provided ID.
        /// </summary>
        /// <param name="id">The internal ID of the background task.</param>
        /// <returns>The background task corresponding to the provided ID.</returns>
        private BackgroundTask GetBackgroundTask(string id)
        {
            // Try to get the background task with the provided ID.
            var backgroundTask = _context.BackgroundTasks
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
        private T GetBackgroundTask<T>(BackgroundTask backgroundTask)
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
        private void DeleteBackgroundTask(BackgroundTask backgroundTask)
        {
            // Mark the task for deletion.
            _context.BackgroundTasks.Remove(backgroundTask);
            // Save the changes.
            _context.SaveChanges();
        }
    }
}
