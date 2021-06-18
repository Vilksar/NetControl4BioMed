using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Implements a recurring task manager.
    /// </summary>
    public class RecurringTaskManager : IRecurringTaskManager
    {
        /// <summary>
        /// Represents the application service provider.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        public RecurringTaskManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Counts the items in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task CountAllItemsAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<RecurringTask>(backgroundTask);
            // Run the task.
            var dictionary = await task.CountAllItemsAsync(_serviceProvider, token);
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new configuration instance.
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            // Go over each entry in the dictionary.
            foreach (var entry in dictionary)
            {
                // Update the counts.
                configuration[$"Data:ItemCount:All:{entry.Key}"] = entry.Value.ToString();
            }
        }

        /// <summary>
        /// Counts the duplicate items in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task CountDuplicateItemsAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<RecurringTask>(backgroundTask);
            // Run the task.
            var dictionary = await task.CountDuplicateItemsAsync(_serviceProvider, token);
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new configuration instance.
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            // Go over each entry in the dictionary.
            foreach (var entry in dictionary)
            {
                // Update the counts.
                configuration[$"Data:ItemCount:Duplicate:{entry.Key}"] = entry.Value.ToString();
            }
        }

        /// <summary>
        /// Counts the orphaned items in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task CountOrphanedItemsAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<RecurringTask>(backgroundTask);
            // Run the task.
            var dictionary = await task.CountOrphanedItemsAsync(_serviceProvider, token);
            // Create a new scope.
            using var scope = _serviceProvider.CreateScope();
            // Use a new configuration instance.
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            // Go over each entry in the dictionary.
            foreach (var entry in dictionary)
            {
                // Update the counts.
                configuration[$"Data:ItemCount:Orphaned:{entry.Key}"] = entry.Value.ToString();
            }
        }

        /// <summary>
        /// Stops the long-running analyses in the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task StopAnalysesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<RecurringTask>(backgroundTask);
            // Run the task.
            await task.StopAnalysesAsync(_serviceProvider, token);
        }

        /// <summary>
        /// Extends the time until the demonstration items are automatically deleted from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task ExtendTimeUntilDeleteDemonstrationItemsAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<RecurringTask>(backgroundTask);
            // Run the task.
            await task.ExtendTimeUntilDeleteDemonstrationItemsAsync(_serviceProvider, token);
        }

        /// <summary>
        /// Alerts the users before deleting the long-standing networks and analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task AlertUsersAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<RecurringTask>(backgroundTask);
            // Run the task.
            await task.AlertUsersAsync(_serviceProvider, token);
        }

        /// <summary>
        /// Deletes the long-standing unconfirmed users from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteUnconfirmedUsersAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<RecurringTask>(backgroundTask);
            // Run the task.
            await task.DeleteUnconfirmedUsersAsync(_serviceProvider, token);
        }

        /// <summary>
        /// Deletes the orphaned items from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteOrphanedItemsAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<RecurringTask>(backgroundTask);
            // Run the task.
            await task.DeleteOrphanedItemsAsync(_serviceProvider, token);
        }

        /// <summary>
        /// Deletes the long-standing networks from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteNetworksAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<RecurringTask>(backgroundTask);
            // Run the task.
            await task.DeleteNetworksAsync(_serviceProvider, token);
        }

        /// <summary>
        /// Deletes the long-standing analyses from the database.
        /// </summary>
        /// <param name="id">The ID of the background task.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAnalysesAsync(string id, CancellationToken token)
        {
            // Get the background task with the provided ID.
            var backgroundTask = GetBackgroundTask(id);
            // Get the task corresponding to the background task.
            var task = GetTask<RecurringTask>(backgroundTask);
            // Run the task.
            await task.DeleteAnalysesAsync(_serviceProvider, token);
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
        /// <typeparam name="T">The type of the background job.</typeparam>
        /// <param name="backgroundTask">The current background task.</param>
        /// <returns>The background job corresponding to the provided background task.</returns>
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
    }
}
