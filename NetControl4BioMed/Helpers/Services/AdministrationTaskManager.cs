using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.BackgroundJobs;
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
        public void CreateNodes(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<CreateNodesBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Creates edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void CreateEdges(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<CreateEdgesBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Creates node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void CreateNodeCollections(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<CreateNodeCollectionsBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Edits nodes in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void EditNodes(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<EditNodesBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Edits edges in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void EditEdges(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<EditEdgesBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Edits node collections in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void EditNodeCollections(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<EditNodeCollectionsBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteUsers(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteUsersBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteRoles(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteRolesBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes user roles from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteUserRoles(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteUserRolesBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes database users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteDatabaseUsers(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteDatabaseUsersBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes database user invitations from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteDatabaseUserInvitations(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteDatabaseUserInvitationsBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes database types from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteDatabaseTypes(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteDatabaseTypesBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes databases from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteDatabases(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteDatabasesBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes database node fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteDatabaseNodeFields(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteDatabaseNodeFieldsBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes database edge fields from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteDatabaseEdgeFields(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteDatabaseEdgeFieldsBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes nodes from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteNodes(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteNodesBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes edges from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteEdges(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteEdgesBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes node collections from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteNodeCollections(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteNodeCollectionsBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteNetworks(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteNetworksBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Deletes analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void DeleteAnalyses(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<DeleteAnalysesBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Stops analyses in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void StopAnalyses(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<StopAnalysesBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
        }

        /// <summary>
        /// Cleans the database of long-standing items.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void Clean(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var task = GetBackgroundTask(id);
            // Get the job corresponding to the task.
            var job = GetBackgroundJob<CleanBackgroundJob>(task);
            // Run the job.
            job.Run(_serviceProvider, token);
            // Complete the task.
            CompleteBackgroundTask(task);
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
            var task = context.BackgroundTasks
                .Where(item => item.Id == id)
                .FirstOrDefault();
            // Check if there was any task found.
            if (task == null)
            {
                // Throw an exception.
                throw new ArgumentException("No task could be found with the provided ID.");
            }
            // Return the task.
            return task;
        }

        /// <summary>
        /// Gets the background job corresponding to the provided background task.
        /// </summary>
        /// <typeparam name="T">The type of the background job.</typeparam>
        /// <param name="task">The current background task.</param>
        /// <returns>The background job corresponding to the provided background task.</returns>
        private T GetBackgroundJob<T>(BackgroundTask task)
        {
            // Try to deserialize the job.
            if (!task.Data.TryDeserializeJsonObject<T>(out var job))
            {
                // Throw an exception.
                throw new ArgumentException("The data of the task could not be deserialized.");
            }
            // Return the job.
            return job;
        }

        /// <summary>
        /// Marks the provided background task as completed in the database.
        /// </summary>
        /// <param name="task">The current background task.</param>
        private void CompleteBackgroundTask(BackgroundTask task)
        {
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new context instance.
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Check if the task is recurring.
            if (task.IsRecurring)
            {
                // End the function.
                return;
            }
            // Mark the task for removal.
            context.BackgroundTasks.Remove(task);
            // Save the changes to the database.
            context.SaveChanges();
        }
    }
}
