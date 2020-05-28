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
    /// Implements a content task manager.
    /// </summary>
    public class ContentTaskManager : IContentTaskManager
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
        public ContentTaskManager(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            _serviceProvider = serviceProvider;
            _context = context;
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
        /// Deletes networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void GenerateNetworks(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetBackgroundTask<NetworksTask>(backgroundTask);
            // Run the task.
            task.Generate(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Deletes analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void GenerateAnalyses(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetBackgroundTask<AnalysesTask>(backgroundTask);
            // Run the task.
            task.Generate(_serviceProvider, token);
            // Complete the task.
            DeleteBackgroundTask(backgroundTask);
        }

        /// <summary>
        /// Stops analyses in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void StartAnalyses(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
           // Get the task corresponding to the background task.
            var task = GetBackgroundTask<AnalysesTask>(backgroundTask);
            // Run the task.
            task.Start(_serviceProvider, token);
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
        /// Sends ended e-mails to the corresponding analysis users.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public void SendEndedEmails(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetBackgroundTask<AnalysesTask>(backgroundTask);
            // Run the task.
            task.SendEndedEmails(_serviceProvider, token);
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
