using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Services;
using NetControl4BioMed.Helpers.Tasks;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Content.Created.Analyses
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public CreateModel(IServiceProvider serviceProvider, UserManager<User> userManager, ApplicationDbContext context, IReCaptchaChecker reCaptchaChecker)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _context = context;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string DatabaseTypeId { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Name { get; set; }

            [DataType(DataType.MultilineText)]
            public string Description { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Algorithm { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string SourceData { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string TargetData { get; set; }

            [Range(0, 10000, ErrorMessage = "The value must be a positive integer lower than 10000.")]
            [Required(ErrorMessage = "This field is required.")]
            public int MaximumIterations { get; set; }

            [Range(0, 1000, ErrorMessage = "The value must be a positive integer lower than 1000.")]
            [Required(ErrorMessage = "This field is required.")]
            public int MaximumIterationsWithoutImprovement { get; set; }

            public Helpers.Algorithms.Algorithm1.Parameters Algorithm1Parameters { get; set; }

            public Helpers.Algorithms.Algorithm2.Parameters Algorithm2Parameters { get; set; }

            public IEnumerable<string> NetworkIds { get; set; }

            public IEnumerable<string> SourceNodeCollectionIds { get; set; }

            public IEnumerable<string> TargetNodeCollectionIds { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public string Algorithm { get; set; }

            public IQueryable<Network> Networks { get; set; }

            public IQueryable<NodeCollection> SourceNodeCollections { get; set; }

            public IQueryable<NodeCollection> TargetNodeCollections { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string databaseTypeId = null, string algorithm = null, string analysisId = null)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if there wasn't any database type ID provided.
            if (string.IsNullOrEmpty(databaseTypeId))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A type is required for creating an analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Try to get the database type with the provided ID.
            var databaseType = _context.DatabaseTypes
                .FirstOrDefault(item => item.Id == databaseTypeId);
            // Check if there wasn't any database type found.
            if (databaseType == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No type could be found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Check if there wasn't any algorithm provided.
            if (algorithm == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An algorithm is required for creating an analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Try to get the algorithm.
            try
            {
                // Get the algorithm.
                _ = EnumerationExtensions.GetEnumerationValue<AnalysisAlgorithm>(algorithm);
            }
            catch (Exception)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The provided algorithm is not valid.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Try to get the analysis with the provided ID.
            var analyses = _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == analysisId);
            // Check if there wasn't any analysis found.
            var analysesFound = analyses != null && analyses.Any();
            // Check if there was an ID provided, but there was no analysis found.
            if (!string.IsNullOrEmpty(analysisId) && !analysesFound)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No analysis could be found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Algorithm = algorithm,
                Networks = _context.Networks
                    .Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType == databaseType))
                    .Where(item => item.NetworkDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user))),
                SourceNodeCollections = _context.NodeCollections
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType == databaseType))
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user))),
                TargetNodeCollections = _context.NodeCollections
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType == databaseType))
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
            };
            // Check if there weren't any networks available.
            if (!View.Networks.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new analysis can't be created, as there are no networks available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the input.
            switch (analysesFound)
            {
                case false:
                    Input = new InputModel
                    {
                        DatabaseTypeId = databaseType.Id,
                        Algorithm = View.Algorithm.ToString(),
                        SourceData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                        TargetData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                        MaximumIterations = 10000,
                        MaximumIterationsWithoutImprovement = 1000,
                        Algorithm1Parameters = View.Algorithm == AnalysisAlgorithm.Algorithm1.ToString() ? new Helpers.Algorithms.Algorithm1.Parameters() : null,
                        Algorithm2Parameters = View.Algorithm == AnalysisAlgorithm.Algorithm2.ToString() ? new Helpers.Algorithms.Algorithm2.Parameters() : null,
                        NetworkIds = Enumerable.Empty<string>(),
                        SourceNodeCollectionIds = Enumerable.Empty<string>(),
                        TargetNodeCollectionIds = Enumerable.Empty<string>()
                    };
                    break;
                case true:
                    Input = new InputModel
                    {
                        DatabaseTypeId = databaseType.Id,
                        Name = analyses
                            .Select(item => item.Name)
                            .FirstOrDefault(),
                        Description = analyses
                            .Select(item => item.Description)
                            .FirstOrDefault(),
                        Algorithm = algorithm,
                        SourceData = JsonSerializer.Serialize(analyses
                            .Select(item => item.AnalysisNodes)
                            .SelectMany(item => item)
                            .Where(item => item.Type == AnalysisNodeType.Source)
                            .Select(item => item.Node.Name)),
                        TargetData = JsonSerializer.Serialize(analyses
                            .Select(item => item.AnalysisNodes)
                            .SelectMany(item => item)
                            .Where(item => item.Type == AnalysisNodeType.Target)
                            .Select(item => item.Node.Name)),
                        MaximumIterations = analyses
                            .Select(item => item.MaximumIterations)
                            .FirstOrDefault(),
                        MaximumIterationsWithoutImprovement = analyses
                            .Select(item => item.MaximumIterationsWithoutImprovement)
                            .FirstOrDefault(),
                        Algorithm1Parameters = algorithm == AnalysisAlgorithm.Algorithm1.ToString() ? JsonSerializer.Deserialize<Helpers.Algorithms.Algorithm1.Parameters>(analyses.Select(item => item.Parameters).FirstOrDefault()) : null,
                        Algorithm2Parameters = algorithm == AnalysisAlgorithm.Algorithm2.ToString() ? JsonSerializer.Deserialize<Helpers.Algorithms.Algorithm2.Parameters>(analyses.Select(item => item.Parameters).FirstOrDefault()) : null,
                        NetworkIds = analyses
                            .Select(item => item.AnalysisNetworks)
                            .SelectMany(item => item)
                            .Select(item => item.Network)
                            .Intersect(View.Networks)
                            .Select(item => item.Id),
                        SourceNodeCollectionIds = analyses
                            .Select(item => item.AnalysisNodeCollections)
                            .SelectMany(item => item)
                            .Where(item => item.Type == AnalysisNodeCollectionType.Source)
                            .Select(item => item.NodeCollection)
                            .Intersect(View.SourceNodeCollections)
                            .Select(item => item.Id),
                        TargetNodeCollectionIds = analyses
                            .Select(item => item.AnalysisNodeCollections)
                            .SelectMany(item => item)
                            .Where(item => item.Type == AnalysisNodeCollectionType.Target)
                            .Select(item => item.NodeCollection)
                            .Intersect(View.TargetNodeCollections)
                            .Select(item => item.Id)
                    };
                    break;
                default:
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if the user does not exist.
            if (user == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An error occured while trying to load the user data. If you are already logged in, please log out and try again.";
                // Redirect to the home page.
                return RedirectToPage("/Index");
            }
            // Check if there isn't any database type ID provided.
            if (string.IsNullOrEmpty(Input.DatabaseTypeId))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A type is required for creating an analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Try to get the database type with the provided ID.
            var databaseType = _context.DatabaseTypes.FirstOrDefault(item => item.Id == Input.DatabaseTypeId);
            // Check if there wasn't any database type found.
            if (databaseType == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No type could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Check if there isn't any algorithm provided.
            if (Input.Algorithm == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An algorithm is required for creating an analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Algorithm = Input.Algorithm,
                Networks = _context.Networks
                    .Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType == databaseType))
                    .Where(item => item.NetworkDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user))),
                SourceNodeCollections = _context.NodeCollections
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType == databaseType))
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user))),
                TargetNodeCollections = _context.NodeCollections
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType == databaseType))
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
            };
            // Check if there weren't any networks available.
            if (!View.Networks.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new analysis can't be created, as there are no networks available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Check if the reCaptcha is valid.
            if (!await _reCaptchaChecker.IsValid(Input.ReCaptchaToken))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The reCaptcha verification failed.");
                // Return the page.
                return Page();
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Redisplay the page.
                return Page();
            }
            // Try to get the algorithm.
            try
            {
                // Get the algorithm.
                var algorithm = EnumerationExtensions.GetEnumerationValue<AnalysisAlgorithm>(Input.Algorithm);
            }
            catch (Exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The analysis algorithm couldn't be determined from the provided string.");
                // Redisplay the page.
                return Page();
            }
            // Check if the parameters match the given algorithm.
            if ((Input.Algorithm == AnalysisAlgorithm.Algorithm1.ToString() && Input.Algorithm1Parameters == null) || (Input.Algorithm == AnalysisAlgorithm.Algorithm2.ToString() && Input.Algorithm2Parameters == null))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The parameter values are not valid for the chosen algorithm.");
                // Redisplay the page.
                return Page();
            }
            // Get the provided network IDs.
            var networkIds = Input.NetworkIds ?? Enumerable.Empty<string>();
            // Check if there weren't any network IDs provided.
            if (!networkIds.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "At least one network must be selected.");
                // Redisplay the page.
                return Page();
            }
            // Try to get the networks with the provided IDs.
            var networks = View.Networks
                .Where(item => networkIds.Contains(item.Id));
            // Check if there weren't any networks found.
            if (!networks.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No networks could be found with the provided IDs.");
                // Redisplay the page.
                return Page();
            }
            // Try to deserialize the seed data.
            if (!Input.SourceData.TryDeserializeJsonObject<IEnumerable<string>>(out var sourceItems) || sourceItems == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided source data could not be deserialized.");
                // Redisplay the page.
                return Page();
            }
            // Get the provided source node collection IDs.
            var sourceNodeCollectionIds = Input.SourceNodeCollectionIds ?? Enumerable.Empty<string>();
            // Try to get the source node collections with the provided IDs.
            var sourceNodeCollections = View.SourceNodeCollections
                .Where(item => sourceNodeCollectionIds.Contains(item.Id));
            // Try to deserialize the seed data.
            if (!Input.TargetData.TryDeserializeJsonObject<IEnumerable<string>>(out var targetItems) || targetItems == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided target data could not be deserialized.");
                // Redisplay the page.
                return Page();
            }
            // Get the provided target node collection IDs.
            var targetNodeCollectionIds = Input.TargetNodeCollectionIds ?? Enumerable.Empty<string>();
            // Try to get the target node collections with the provided IDs.
            var targetNodeCollections = View.TargetNodeCollections
                .Where(item => targetNodeCollectionIds.Contains(item.Id));
            // Check if there wasn't any target data found.
            if (!targetItems.Any() || !targetNodeCollections.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No target data has been provided.");
                // Redisplay the page.
                return Page();
            }
            // Define the generation data.
            var data = JsonSerializer.Serialize(sourceItems
                .Select(item => new AnalysisNodeInputModel
                {
                    Node = new NodeInputModel
                    {
                        Id = item
                    },
                    Type = "Source"
                })
                .Concat(targetItems
                    .Select(item => new AnalysisNodeInputModel
                    {
                        Node = new NodeInputModel
                        {
                            Id = item
                        },
                        Type = "Target"
                    })));
            // Define a new task.
            var task = new AnalysesTask
            {
                Items = new List<AnalysisInputModel>
                {
                    new AnalysisInputModel
                    {
                        Name = Input.Name,
                        Description = Input.Description,
                        Data = data,
                        MaximumIterations = Input.MaximumIterations,
                        MaximumIterationsWithoutImprovement = Input.MaximumIterationsWithoutImprovement,
                        Algorithm = Input.Algorithm,
                        Parameters = Input.Algorithm == AnalysisAlgorithm.Algorithm1.ToString() ? JsonSerializer.Serialize(Input.Algorithm1Parameters, new JsonSerializerOptions { IgnoreReadOnlyProperties = true }) :
                            Input.Algorithm == AnalysisAlgorithm.Algorithm2.ToString() ? JsonSerializer.Serialize(Input.Algorithm2Parameters, new JsonSerializerOptions { IgnoreReadOnlyProperties = true }) :
                            null,
                        AnalysisUsers = new List<AnalysisUserInputModel>
                        {
                            new AnalysisUserInputModel
                            {
                                User = new UserInputModel
                                {
                                    Id = user.Id
                                }
                            }
                        },
                        AnalysisNodeCollections = sourceNodeCollections
                            .Select(item => item.Id)
                            .Select(item => new AnalysisNodeCollectionInputModel
                            {
                                NodeCollection = new NodeCollectionInputModel
                                {
                                    Id = item
                                },
                                Type = "Source"
                            })
                            .Concat(targetNodeCollections
                                .Select(item => item.Id)
                                .Select(item => new AnalysisNodeCollectionInputModel
                                {
                                    NodeCollection = new NodeCollectionInputModel
                                    {
                                        Id = item
                                    },
                                    Type = "Target"
                                })),
                        AnalysisNetworks = networks
                            .Select(item => item.Id)
                            .Select(item => new AnalysisNetworkInputModel
                            {
                                Network = new NetworkInputModel
                                {
                                    Id = item
                                }
                            })
                    }
                }
            };
            // Define a variable to store the analyses that will be created.
            var analyses = new List<Analysis>();
            // Try to run the task.
            try
            {
                // Run the task.
                analyses.AddRange(task.Create(_serviceProvider, CancellationToken.None).ToList());
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Define the new background tasks.
            var generateBackgroundTask = new BackgroundTask
            {
                DateTimeCreated = DateTime.Now,
                Name = $"{nameof(IContentTaskManager)}.{nameof(IContentTaskManager.GenerateAnalyses)}",
                IsRecurring = false,
                Data = JsonSerializer.Serialize(new AnalysesTask
                {
                    Items = analyses.Select(item => new AnalysisInputModel
                    {
                        Id = item.Id
                    })
                })
            };
            var startBackgroundTask = new BackgroundTask
            {
                DateTimeCreated = DateTime.Now,
                Name = $"{nameof(IContentTaskManager)}.{nameof(IContentTaskManager.StartAnalyses)}",
                IsRecurring = false,
                Data = JsonSerializer.Serialize(new AnalysesTask
                {
                    Scheme = HttpContext.Request.Scheme,
                    HostValue = HttpContext.Request.Host.Value,
                    Items = analyses.Select(item => new AnalysisInputModel
                    {
                        Id = item.Id
                    })
                })
            };
            // Mark the task for addition.
            _context.BackgroundTasks.Add(generateBackgroundTask);
            _context.BackgroundTasks.Add(startBackgroundTask);
            // Save the changes to the database.
            _context.SaveChanges();
            // Create a new Hangfire background job.
            var jobId = BackgroundJob.Enqueue<IContentTaskManager>(item => item.GenerateAnalyses(generateBackgroundTask.Id, CancellationToken.None));
            var continuationJobId = BackgroundJob.ContinueJobWith<IContentTaskManager>(jobId, item => item.StartAnalyses(startBackgroundTask.Id, CancellationToken.None), JobContinuationOptions.OnlyOnSucceededState);
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 analysis of type \"{databaseType.Name}\" with algorithm \"{Input.Algorithm}\" defined successfully and scheduled for generation.";
            // Redirect to the index page.
            return RedirectToPage("/Content/Created/Analyses/Index");
        }
    }
}
