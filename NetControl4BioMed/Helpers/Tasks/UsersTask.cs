using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Exceptions;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Tasks
{
    /// <summary>
    /// Implements a task to update users in the database.
    /// </summary>
    public class UsersTask
    {
        /// <summary>
        /// Gets or sets the items to be updated.
        /// </summary>
        public IEnumerable<UserInputModel> Items { get; set; }

        /// <summary>
        /// Creates the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task CreateAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new TaskException("No valid items could be found with the provided data.");
            }
            // Check if the exception item should be shown.
            var showExceptionItem = Items.Count() > 1;
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / ApplicationDbContext.BatchSize);
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
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id);
                var batchEmails = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Email))
                    .Select(item => item.Id)
                    .Distinct();
                // Check if any of the IDs are repeating in the list.
                if (batchIds.Distinct().Count() != batchIds.Count())
                {
                    // Throw an exception.
                    throw new TaskException("Two or more of the manually provided IDs are duplicated.");
                }
                // Define the list of items to get.
                var validBatchIds = new List<string>();
                // Define the dependent list of items to get.
                var databaseUserInvitationInputs = new List<DatabaseUserInvitationInputModel>();
                var networkUserInvitationInputs = new List<NetworkUserInvitationInputModel>();
                var analysisUserInvitationInputs = new List<AnalysisUserInvitationInputModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the valid IDs, that do not appear in the database.
                    validBatchIds = batchIds
                        .Except(context.Users
                            .Where(item => batchIds.Contains(item.Id))
                            .Select(item => item.Id))
                        .ToList();
                    // Get the IDs of the dependent items.
                    databaseUserInvitationInputs = context.DatabaseUserInvitations
                        .Where(item => batchEmails.Contains(item.Email))
                        .Distinct()
                        .Select(item => new DatabaseUserInvitationInputModel
                        {
                            Database = new DatabaseInputModel
                            {
                                Id = item.Database.Id
                            },
                            Email = item.Email
                        })
                        .ToList();
                    networkUserInvitationInputs = context.NetworkUserInvitations
                        .Where(item => batchEmails.Contains(item.Email))
                        .Distinct()
                        .Select(item => new NetworkUserInvitationInputModel
                        {
                            Network = new NetworkInputModel
                            {
                                Id = item.Network.Id
                            },
                            Email = item.Email
                        })
                        .ToList();
                    analysisUserInvitationInputs = context.AnalysisUserInvitations
                        .Where(item => batchEmails.Contains(item.Email))
                        .Distinct()
                        .Select(item => new AnalysisUserInvitationInputModel
                        {
                            Analysis = new AnalysisInputModel
                            {
                                Id = item.Analysis.Id
                            },
                            Email = item.Email
                        })
                        .ToList();
                }
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new user manager instance.
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                    // Go over each item in the current batch.
                    foreach (var batchItem in batchItems)
                    {
                        // Check if the ID of the item is not valid.
                        if (!string.IsNullOrEmpty(batchItem.Id) && !validBatchIds.Contains(batchItem.Id))
                        {
                            // Continue.
                            continue;
                        }
                        // Define the corresponding item.
                        var user = new User
                        {
                            UserName = batchItem.Email,
                            Email = batchItem.Email,
                            DateTimeCreated = DateTime.UtcNow
                        };
                        // Check if there is any ID provided.
                        if (!string.IsNullOrEmpty(batchItem.Id))
                        {
                            // Assign it to the item.
                            user.Id = batchItem.Id;
                        }
                        // Define a new identity result.
                        var result = IdentityResult.Success;
                        // Check the type of the item.
                        if (batchItem.Type == "None")
                        {
                            // Try to create the new user.
                            result = result.Succeeded ? await userManager.CreateAsync(user) : result;
                        }
                        else if (batchItem.Type == "Password")
                        {
                            // Try to get the passsord from the data.
                            if (!batchItem.Data.TryDeserializeJsonObject<string>(out var password))
                            {
                                // Throw an exception.
                                throw new TaskException("The provided data couldn't be deserialized.", showExceptionItem, batchItem);
                            }
                            // Try to create the new user.
                            result = result.Succeeded ? await userManager.CreateAsync(user, password) : result;
                        }
                        else if (batchItem.Type == "External")
                        {
                            // Try to get the passsord from the data.
                            if (!batchItem.Data.TryDeserializeJsonObject<ExternalLoginInfo>(out var info))
                            {
                                // Throw an exception.
                                throw new TaskException("The provided data couldn't be deserialized.", showExceptionItem, batchItem);
                            }
                            // Try to create the new user.
                            result = result.Succeeded ? await userManager.CreateAsync(user) : result;
                            // Add the external login.
                            result = result.Succeeded ? await userManager.AddLoginAsync(user, info) : result;
                        }
                        else
                        {
                            // Throw an exception.
                            throw new TaskException("The provided data type is invalid.", showExceptionItem, batchItem);
                        }
                        // Check if the e-mail should be set as confirmed.
                        if (batchItem.EmailConfirmed)
                        {
                            // Generate the token for e-mail confirmation.
                            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                            // Confirm the e-mail address.
                            result = result.Succeeded ? await userManager.ConfirmEmailAsync(user, confirmationToken) : result;
                        }
                        // Check if any of the operations has failed.
                        if (!result.Succeeded)
                        {
                            // Define the exception messages.
                            var messages = result.Errors
                                .Select(item => item.Description);
                            // Throw an exception.
                            throw new TaskException(string.Join(" ", messages), showExceptionItem, batchItem);
                        }
                        // Create the dependent entities.
                        await new DatabaseUsersTask
                        {
                            Items = databaseUserInvitationInputs
                                .Where(item => item.Email == user.Email)
                                .Select(item => new DatabaseUserInputModel
                                {
                                    Database = new DatabaseInputModel
                                    {
                                        Id = item.Database.Id
                                    },
                                    User = new UserInputModel
                                    {
                                        Id = user.Id
                                    }
                                })
                        }.CreateAsync(serviceProvider, token);
                        await new NetworkUsersTask
                        {
                            Items = networkUserInvitationInputs
                                .Where(item => item.Email == user.Email)
                                .Select(item => new NetworkUserInputModel
                                {
                                    Network = new NetworkInputModel
                                    {
                                        Id = item.Network.Id
                                    },
                                    User = new UserInputModel
                                    {
                                        Id = user.Id
                                    }
                                })
                        }.CreateAsync(serviceProvider, token);
                        await new AnalysisUsersTask
                        {
                            Items = analysisUserInvitationInputs
                                 .Where(item => item.Email == user.Email)
                                 .Select(item => new AnalysisUserInputModel
                                 {
                                     Analysis = new AnalysisInputModel
                                     {
                                         Id = item.Analysis.Id
                                     },
                                     User = new UserInputModel
                                     {
                                         Id = user.Id
                                     }
                                 })
                        }.CreateAsync(serviceProvider, token);
                        // Delete the dependent entities.
                        await new DatabaseUserInvitationsTask
                        {
                            Items = databaseUserInvitationInputs
                                 .Where(item => item.Email == user.Email)
                        }.DeleteAsync(serviceProvider, token);
                        await new NetworkUserInvitationsTask
                        {
                            Items = networkUserInvitationInputs
                                 .Where(item => item.Email == user.Email)
                        }.DeleteAsync(serviceProvider, token);
                        await new AnalysisUserInvitationsTask
                        {
                            Items = analysisUserInvitationInputs
                                 .Where(item => item.Email == user.Email)
                        }.DeleteAsync(serviceProvider, token);
                    }
                }
            }
        }

        /// <summary>
        /// Edits the items in the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task EditAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new TaskException("No valid items could be found with the provided data.");
            }
            // Check if the exception item should be shown.
            var showExceptionItem = Items.Count() > 1;
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / ApplicationDbContext.BatchSize);
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
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Id))
                    .Select(item => item.Id)
                    .Distinct();
                var batchEmails = batchItems
                    .Where(item => !string.IsNullOrEmpty(item.Email))
                    .Select(item => item.Id)
                    .Distinct();
                // Define the list of items to get.
                var users = new List<User>();
                // Define the dependent list of items to get.
                var databaseUserInvitationInputs = new List<DatabaseUserInvitationInputModel>();
                var networkUserInvitationInputs = new List<NetworkUserInvitationInputModel>();
                var analysisUserInvitationInputs = new List<AnalysisUserInvitationInputModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Users
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    users = items
                        .ToList();
                    // Get the IDs of the dependent items.
                    databaseUserInvitationInputs = context.DatabaseUserInvitations
                        .Where(item => batchEmails.Contains(item.Email))
                        .Distinct()
                        .Select(item => new DatabaseUserInvitationInputModel
                        {
                            Database = new DatabaseInputModel
                            {
                                Id = item.Database.Id
                            },
                            Email = item.Email
                        })
                        .ToList();
                    networkUserInvitationInputs = context.NetworkUserInvitations
                        .Where(item => batchEmails.Contains(item.Email))
                        .Distinct()
                        .Select(item => new NetworkUserInvitationInputModel
                        {
                            Network = new NetworkInputModel
                            {
                                Id = item.Network.Id
                            },
                            Email = item.Email
                        })
                        .ToList();
                    analysisUserInvitationInputs = context.AnalysisUserInvitations
                        .Where(item => batchEmails.Contains(item.Email))
                        .Distinct()
                        .Select(item => new AnalysisUserInvitationInputModel
                        {
                            Analysis = new AnalysisInputModel
                            {
                                Id = item.Analysis.Id
                            },
                            Email = item.Email
                        })
                        .ToList();
                }
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new user manager instance.
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                    // Go over each item in the current batch.
                    foreach (var batchItem in batchItems)
                    {
                        // Get the corresponding item.
                        var user = users
                            .FirstOrDefault(item => item.Id == batchItem.Id);
                        // Check if there was no item found.
                        if (user == null)
                        {
                            // Continue.
                            continue;
                        }
                        // Define a new identity result.
                        var result = IdentityResult.Success;
                        // Check if the e-mail is different from the current e-mail.
                        if (batchItem.Email != user.Email)
                        {
                            // Try to update the e-mail.
                            result = result.Succeeded ? await userManager.SetEmailAsync(user, batchItem.Email) : result;
                        }
                        // Check if the e-mail is different from the current username.
                        if (batchItem.Email != user.UserName)
                        {
                            // Try to update the username.
                            result = result.Succeeded ? await userManager.SetUserNameAsync(user, batchItem.Email) : result;
                        }
                        // Check if the e-mail should be set as confirmed.
                        if (!user.EmailConfirmed && batchItem.EmailConfirmed)
                        {
                            // Generate the token and try to set it.
                            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                            result = result.Succeeded ? await userManager.ConfirmEmailAsync(user, confirmationToken) : result;
                        }
                        // Check if any of the operations has failed.
                        if (!result.Succeeded)
                        {
                            // Define the exception messages.
                            var messages = result.Errors
                                .Select(item => item.Description);
                            // Throw an exception.
                            throw new TaskException(string.Join(" ", messages), showExceptionItem, batchItem);
                        }
                        // Create the dependent entities.
                        await new DatabaseUsersTask
                        {
                            Items = databaseUserInvitationInputs
                                .Where(item => item.Email == user.Email)
                                .Select(item => new DatabaseUserInputModel
                                {
                                    Database = new DatabaseInputModel
                                    {
                                        Id = item.Database.Id
                                    },
                                    User = new UserInputModel
                                    {
                                        Id = user.Id
                                    }
                                })
                        }.CreateAsync(serviceProvider, token);
                        await new NetworkUsersTask
                        {
                            Items = networkUserInvitationInputs
                                .Where(item => item.Email == user.Email)
                                .Select(item => new NetworkUserInputModel
                                {
                                    Network = new NetworkInputModel
                                    {
                                        Id = item.Network.Id
                                    },
                                    User = new UserInputModel
                                    {
                                        Id = user.Id
                                    }
                                })
                        }.CreateAsync(serviceProvider, token);
                        await new AnalysisUsersTask
                        {
                            Items = analysisUserInvitationInputs
                                 .Where(item => item.Email == user.Email)
                                 .Select(item => new AnalysisUserInputModel
                                 {
                                     Analysis = new AnalysisInputModel
                                     {
                                         Id = item.Analysis.Id
                                     },
                                     User = new UserInputModel
                                     {
                                         Id = user.Id
                                     }
                                 })
                        }.CreateAsync(serviceProvider, token);
                        // Delete the dependent entities.
                        await new DatabaseUserInvitationsTask
                        {
                            Items = databaseUserInvitationInputs
                                 .Where(item => item.Email == user.Email)
                        }.DeleteAsync(serviceProvider, token);
                        await new NetworkUserInvitationsTask
                        {
                            Items = networkUserInvitationInputs
                                 .Where(item => item.Email == user.Email)
                        }.DeleteAsync(serviceProvider, token);
                        await new AnalysisUserInvitationsTask
                        {
                            Items = analysisUserInvitationInputs
                                 .Where(item => item.Email == user.Email)
                        }.DeleteAsync(serviceProvider, token);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the items from the database.
        /// </summary>
        /// <param name="serviceProvider">The application service provider.</param>
        /// <param name="token">The cancellation token for the task.</param>
        public async Task DeleteAsync(IServiceProvider serviceProvider, CancellationToken token)
        {
            // Check if there weren't any valid items found.
            if (Items == null)
            {
                // Throw an exception.
                throw new TaskException("No valid items could be found with the provided data.");
            }
            // Get the total number of batches.
            var count = Math.Ceiling((double)Items.Count() / ApplicationDbContext.BatchSize);
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
                var batchItems = Items
                    .Skip(index * ApplicationDbContext.BatchSize)
                    .Take(ApplicationDbContext.BatchSize);
                // Get the IDs of the items in the current batch.
                var batchIds = batchItems.Select(item => item.Id);
                // Define the list of items to get.
                var users = new List<User>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items with the provided IDs.
                    var items = context.Users
                        .Where(item => batchIds.Contains(item.Id));
                    // Check if there were no items found.
                    if (items == null || !items.Any())
                    {
                        // Continue.
                        continue;
                    }
                    // Get the items found.
                    users = items
                        .ToList();
                }
                // Get the IDs of the items.
                var userIds = users
                    .Select(item => item.Id);
                // Define the dependent list of items to get.
                var analysisInputs = new List<AnalysisInputModel>();
                var networkInputs = new List<NetworkInputModel>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new context instance.
                    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Get the items found.
                    var networks = context.Networks
                        .Where(item => item.NetworkUsers.All(item1 => userIds.Contains(item1.User.Id)))
                        .Select(item => item.NetworkEdges)
                        .SelectMany(item => item)
                        .Select(item => item.Network)
                        .Distinct();
                    // Get the IDs of the dependent items.
                    analysisInputs = context.Analyses
                        .Where(item => item.AnalysisUsers.All(item1 => userIds.Contains(item1.User.Id)) || item.AnalysisNetworks.Any(item1 => networks.Contains(item1.Network)))
                        .Select(item => item.AnalysisEdges)
                        .SelectMany(item => item)
                        .Select(item => item.Analysis)
                        .Distinct()
                        .Select(item => new AnalysisInputModel
                        {
                            Id = item.Id
                        })
                        .ToList();
                    networkInputs = networks
                        .Select(item => new NetworkInputModel
                        {
                            Id = item.Id
                        })
                        .ToList();
                }
                // Delete the dependent entities.
                await new AnalysesTask { Items = analysisInputs }.DeleteAsync(serviceProvider, token);
                await new NetworksTask { Items = networkInputs }.DeleteAsync(serviceProvider, token);
                // Delete the related entities.
                await UserExtensions.DeleteRelatedEntitiesAsync<AnalysisUser>(userIds, serviceProvider, token);
                await UserExtensions.DeleteRelatedEntitiesAsync<NetworkUser>(userIds, serviceProvider, token);
                await UserExtensions.DeleteRelatedEntitiesAsync<DatabaseUser>(userIds, serviceProvider, token);
                // Define a variable to store the error messages.
                var errorMessages = new List<string>();
                // Use a new scope.
                using (var scope = serviceProvider.CreateScope())
                {
                    // Use a new user manager instance.
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                    // Go over each item.
                    foreach (var user in users)
                    {
                        // Delete it.
                        var result = await userManager.DeleteAsync(user);
                        // Check if the operation has failed.
                        if (!result.Succeeded)
                        {
                            // Define the exception messages.
                            var messages = result.Errors
                                .Select(item => item.Description);
                            // Add the exception messages to the error messages.
                            errorMessages.AddRange(messages);
                        }
                    }
                }
                // Check if there have been any error messages.
                if (errorMessages.Any())
                {
                    // Throw an exception.
                    throw new TaskException(string.Join(" ", errorMessages));
                }
            }
        }
    }
}
