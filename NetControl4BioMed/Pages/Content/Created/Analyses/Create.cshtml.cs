using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
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
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Services;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Content.Created.Analyses
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;
        private readonly IAnalysisRunner _analysisRunner;

        public CreateModel(UserManager<User> userManager, ApplicationDbContext context, LinkGenerator linkGenerator, IAnalysisRunner analysisRunner)
        {
            _userManager = userManager;
            _context = context;
            _linkGenerator = linkGenerator;
            _analysisRunner = analysisRunner;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel : IValidatableObject
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
            public AnalysisAlgorithm? Algorithm { get; set; }

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

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                // Check if there isn't any algorithm
                if (Algorithm == null)
                {
                    // Return an error.
                    yield return new ValidationResult("The algorithm value is not valid.", new List<string> { string.Empty });
                }
                // Check if the parameters match the given algorithm.
                if ((Algorithm == AnalysisAlgorithm.Algorithm1 && Algorithm1Parameters == null) || (Algorithm == AnalysisAlgorithm.Algorithm2 && Algorithm2Parameters == null))
                {
                    // Return an error.
                    yield return new ValidationResult("The parameter values are not valid for the chosen algorithm.", new List<string> { string.Empty });
                }
            }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public AnalysisAlgorithm Algorithm { get; set; }

            public IQueryable<Network> Networks { get; set; }

            public IQueryable<NodeCollection> SourceNodeCollections { get; set; }

            public IQueryable<NodeCollection> TargetNodeCollections { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string databaseTypeId = null, AnalysisAlgorithm? algorithm = null, string analysisId = null)
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
            // Try to get the analysis with the provided ID.
            var analysis = !string.IsNullOrEmpty(analysisId) ? _context.Analyses
                .Where(item => item.AnalysisUsers.Any(item1 => item1.User == user))
                .FirstOrDefault(item => item.Id == analysisId) : null;
            // Check if there was no analysis found.
            if (!string.IsNullOrEmpty(analysisId) && analysis == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No analysis could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Update the database type ID.
            databaseTypeId = analysis == null ? databaseTypeId : analysis.AnalysisDatabases.FirstOrDefault()?.Database?.DatabaseType?.Id;
            // Check if there isn't any database type ID provided or if the database type ID couldn't be inferred.
            if (string.IsNullOrEmpty(databaseTypeId))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A type is required for creating an analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Try to get the database type with the provided ID.
            var databaseType = _context.DatabaseTypes.FirstOrDefault(item => item.Id == databaseTypeId);
            // Check if there wasn't any database type found.
            if (databaseType == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No type could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Update the algorithm.
            algorithm = analysis == null ? algorithm : analysis.Algorithm;
            // Check if there isn't any algorithm provided.
            if (algorithm == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: An algorithm is required for creating an analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Algorithm = algorithm.Value,
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
            Input = analysis == null ?
                new InputModel
                {
                    DatabaseTypeId = databaseType.Id,
                    Algorithm = View.Algorithm,
                    SourceData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                    TargetData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                    MaximumIterations = 10000,
                    MaximumIterationsWithoutImprovement = 1000,
                    Algorithm1Parameters = View.Algorithm == AnalysisAlgorithm.Algorithm1 ? new Helpers.Algorithms.Algorithm1.Parameters() : null,
                    Algorithm2Parameters = View.Algorithm == AnalysisAlgorithm.Algorithm2 ? new Helpers.Algorithms.Algorithm2.Parameters() : null,
                    NetworkIds = Enumerable.Empty<string>(),
                    SourceNodeCollectionIds = Enumerable.Empty<string>(),
                    TargetNodeCollectionIds = Enumerable.Empty<string>()
                } :
                new InputModel
                {
                    DatabaseTypeId = databaseType.Id,
                    Name = analysis.Name,
                    Description = analysis.Description,
                    Algorithm = analysis.Algorithm,
                    SourceData = JsonSerializer.Serialize(analysis.AnalysisNodes.Where(item => item.Type == AnalysisNodeType.Source).Select(item => item.Node.Name)),
                    TargetData = JsonSerializer.Serialize(analysis.AnalysisNodes.Where(item => item.Type == AnalysisNodeType.Target).Select(item => item.Node.Name)),
                    MaximumIterations = analysis.MaximumIterations,
                    MaximumIterationsWithoutImprovement = analysis.MaximumIterationsWithoutImprovement,
                    Algorithm1Parameters = analysis.Algorithm == AnalysisAlgorithm.Algorithm1 ? JsonSerializer.Deserialize<Helpers.Algorithms.Algorithm1.Parameters>(analysis.Parameters) : null,
                    Algorithm2Parameters = analysis.Algorithm == AnalysisAlgorithm.Algorithm2 ? JsonSerializer.Deserialize<Helpers.Algorithms.Algorithm2.Parameters>(analysis.Parameters) : null,
                    NetworkIds = analysis.AnalysisNetworks
                        .Select(item => item.Network)
                        .Intersect(View.Networks)
                        .Select(item => item.Id),
                    SourceNodeCollectionIds = analysis.AnalysisNodeCollections
                        .Where(item => item.Type == AnalysisNodeCollectionType.Source)
                        .Select(item => item.NodeCollection)
                        .Intersect(View.SourceNodeCollections)
                        .Select(item => item.Id),
                    TargetNodeCollectionIds = analysis.AnalysisNodeCollections
                        .Where(item => item.Type == AnalysisNodeCollectionType.Target)
                        .Select(item => item.NodeCollection)
                        .Intersect(View.TargetNodeCollections)
                        .Select(item => item.Id)
                };
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
            // Check if there isn't any ID provided.
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
                Algorithm = Input.Algorithm.Value,
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
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
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
            // Get the databases to which the user has access.
            var databases = _context.Databases
                .Where(item => item.DatabaseType == databaseType)
                .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user));
            // Try to deserialize the source data.
            if (!Input.SourceData.TryDeserializeJsonObject<IEnumerable<string>>(out var sourceItems))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided source data is not a valid JSON object of nodes.");
                // Redisplay the page.
                return Page();
            }
            // Get the provided source node collection IDs.
            var sourceNodeCollectionIds = Input.SourceNodeCollectionIds ?? Enumerable.Empty<string>();
            // Try to get the source node collections with the provided IDs.
            var sourceNodeCollections = View.SourceNodeCollections
                .Where(item => sourceNodeCollectionIds.Contains(item.Id));
            // Get all of the source nodes that match the given data.
            var sourceNodes = networks
                .Select(item => item.NetworkNodes)
                .SelectMany(item => item)
                .Select(item => item.Node)
                .Where(item => item.DatabaseNodeFieldNodes.Where(item1 => databases.Contains(item1.DatabaseNodeField.Database) && item1.DatabaseNodeField.IsSearchable).Any(item1 => sourceItems.Contains(item1.Node.Id) || sourceItems.Contains(item1.Value)))
                .Concat(sourceNodeCollections
                    .Select(item => item.NodeCollectionNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Node))
                .Distinct();
            // Try to deserialize the target data.
            if (!Input.TargetData.TryDeserializeJsonObject<IEnumerable<string>>(out var targetItems))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided target data is not a valid JSON object of nodes.");
                // Redisplay the page.
                return Page();
            }
            // Get the provided source node collection IDs.
            var targetNodeCollectionIds = Input.TargetNodeCollectionIds ?? Enumerable.Empty<string>();
            // Try to get the target node collections with the provided IDs.
            var targetNodeCollections = View.TargetNodeCollections
                .Where(item => targetNodeCollectionIds.Contains(item.Id));
            // Get all of the target nodes that match the given data.
            var targetNodes = networks
                .Select(item => item.NetworkNodes)
                .SelectMany(item => item)
                .Select(item => item.Node)
                .Where(item => item.DatabaseNodeFieldNodes.Where(item1 => databases.Contains(item1.DatabaseNodeField.Database) && item1.DatabaseNodeField.IsSearchable).Any(item1 => targetItems.Contains(item1.Node.Id) || targetItems.Contains(item1.Value)))
                .Concat(targetNodeCollections
                    .Select(item => item.NodeCollectionNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Node))
                .Distinct();
            // Check if there haven't been any target nodes found.
            if (targetNodes == null || !targetNodes.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No target nodes could be found with the provided target data.");
                // Redisplay the page.
                return Page();
            }
            // Define the JSON serializer options.
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreReadOnlyProperties = true
            };
            // Define the new analysis.
            var analysis = new Analysis
            {
                Name = Input.Name,
                Description = Input.Description,
                DateTimeStarted = null,
                DateTimeEnded = null,
                Algorithm = View.Algorithm,
                CurrentIteration = 0,
                CurrentIterationWithoutImprovement = 0,
                MaximumIterations = Input.MaximumIterations,
                MaximumIterationsWithoutImprovement = Input.MaximumIterationsWithoutImprovement,
                Parameters = View.Algorithm == AnalysisAlgorithm.Algorithm1 ? JsonSerializer.Serialize(Input.Algorithm1Parameters, jsonSerializerOptions) :
                    View.Algorithm == AnalysisAlgorithm.Algorithm2 ? JsonSerializer.Serialize(Input.Algorithm2Parameters, jsonSerializerOptions) : null,
                Status = AnalysisStatus.Scheduled,
                Log = JsonSerializer.Serialize(new List<string> { $"{DateTime.Now}: Analysis has been created and scheduled." }),
                AnalysisUsers = new List<AnalysisUser>
                {
                    new AnalysisUser
                    {
                        User = user,
                        DateTimeCreated = DateTime.Now
                    }
                },
                AnalysisDatabases = networks
                    .Select(item => item.NetworkDatabases)
                    .SelectMany(item => item)
                    .Select(item => item.Database)
                    .Distinct()
                    .Select(item => new AnalysisDatabase
                    {
                        Database = item
                    })
                    .ToList(),
                AnalysisNodes = networks
                    .Select(item => item.NetworkNodes)
                    .SelectMany(item => item)
                    .Select(item => item.Node)
                    .Distinct()
                    .Select(item => new AnalysisNode
                    {
                        Node = item,
                        Type = AnalysisNodeType.None
                    })
                    .Concat(sourceNodes
                        .Select(item => new AnalysisNode
                        {
                            Node = item,
                            Type = AnalysisNodeType.Source
                        }))
                    .Concat(targetNodes
                        .Select(item => new AnalysisNode
                        {
                            Node = item,
                            Type = AnalysisNodeType.Target
                        }))
                    .ToList(),
                AnalysisEdges = networks
                    .Select(item => item.NetworkEdges)
                    .SelectMany(item => item)
                    .Select(item => item.Edge)
                    .Distinct()
                    .Select(item => new AnalysisEdge
                    {
                        Edge = item
                    })
                    .ToList(),
                AnalysisNodeCollections = sourceNodeCollections
                    .Select(item => new AnalysisNodeCollection
                    {
                        NodeCollection = item,
                        Type = AnalysisNodeCollectionType.Source
                    })
                    .Concat(targetNodeCollections
                        .Select(item => new AnalysisNodeCollection
                        {
                            NodeCollection = item,
                            Type = AnalysisNodeCollectionType.Target
                        }))
                    .ToList(),
                AnalysisNetworks = networks
                    .Select(item => new AnalysisNetwork
                    {
                        Network = item
                    })
                    .ToList()
            };
            // Mark the data for addition.
            _context.Analyses.Add(analysis);
            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Define the analysis runner view model.
            var viewModel = new AnalysisRunnerViewModel
            {
                Id = analysis.Id,
                Url = _linkGenerator.GetUriByPage(HttpContext, "/Index", handler: null, values: null),
                ApplicationUrl = _linkGenerator.GetUriByPage(HttpContext, "/Content/Created/Analyses/Details/Index", handler: null, values: new { id = analysis.Id })
            };
            // Add a new Hangfire background task.
            //BackgroundJob.Enqueue(() => _analysisRunner.Run(viewModel));
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 analysis of type \"{databaseType.Name}\" using the algorithm \"{analysis.Algorithm.GetDisplayName()}\" created and scheduled successfully.";
            // Redirect to the index page.
            return RedirectToPage("/Content/Created/Analyses/Index");
        }
    }
}
