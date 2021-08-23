using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EnumerationProteinCollectionType = NetControl4BioMed.Data.Enumerations.ProteinCollectionType;
using GeneticAlgorithm = NetControl4BioMed.Helpers.Algorithms.Analyses.Genetic;
using GreedyAlgorithm = NetControl4BioMed.Helpers.Algorithms.Analyses.Greedy;

namespace NetControl4BioMed.Pages.CreatedData.Analyses
{
    [RequestFormLimits(ValueLengthLimit = 16 * 1024 * 1024)]
    public class CreateModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public CreateModel(IServiceProvider serviceProvider, UserManager<User> userManager, ApplicationDbContext context, IConfiguration configuration, IReCaptchaChecker reCaptchaChecker)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel : IValidatableObject
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Name { get; set; }

            [DataType(DataType.MultilineText)]
            public string Description { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public bool IsPublic { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string NetworkId { get; set; }

            public bool UseSourceProteinData { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string SourceProteinData { get; set; }

            public bool UseSourceProteinCollectionData { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string SourceProteinCollectionData { get; set; }

            public bool UseTargetProteinData { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string TargetProteinData { get; set; }

            public bool UseTargetProteinCollectionData { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string TargetProteinCollectionData { get; set; }

            [Range(0, 10000, ErrorMessage = "The value must be a positive integer lower than 10000.")]
            [Required(ErrorMessage = "This field is required.")]
            public int MaximumIterations { get; set; }

            [Range(0, 1000, ErrorMessage = "The value must be a positive integer lower than 1000.")]
            [Required(ErrorMessage = "This field is required.")]
            public int MaximumIterationsWithoutImprovement { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Algorithm { get; set; }

            public bool UseDefaultAlgorithmParameters { get; set; }

            public GreedyAlgorithm.Parameters GreedyAlgorithmParameters { get; set; }

            public GeneticAlgorithm.Parameters GeneticAlgorithmParameters { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string ReCaptchaToken { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                // Check the selected algorithm.
                if (Algorithm == AnalysisAlgorithm.Greedy.ToString())
                {
                    // Check if the parameters don't match the algorithm.
                    if (GreedyAlgorithmParameters == null)
                    {
                        // Return an error.
                        yield return new ValidationResult("The parameters do not match the chosen algorithm.", new List<string> { string.Empty });
                    }
                    // Get the validation results for the parameters.
                    var validationResults = GreedyAlgorithmParameters.Validate(validationContext);
                    // Go over each validation error.
                    foreach (var validationResult in validationResults)
                    {
                        // Return an error.
                        yield return new ValidationResult(validationResult.ErrorMessage, validationResult.MemberNames.Select(item => $"Input.{nameof(GreedyAlgorithmParameters)}.{item}"));
                    }
                }
                else if (Algorithm == AnalysisAlgorithm.Genetic.ToString())
                {
                    // Check if the parameters don't match the algorithm.
                    if (GeneticAlgorithmParameters == null)
                    {
                        // Return an error.
                        yield return new ValidationResult("The parameters do not match the chosen algorithm.", new List<string> { string.Empty });
                    }
                    // Get the validation results for the parameters.
                    var validationResults = GeneticAlgorithmParameters.Validate(validationContext);
                    // Go over each validation error.
                    foreach (var validationResult in validationResults)
                    {
                        // Return an error.
                        yield return new ValidationResult(validationResult.ErrorMessage, validationResult.MemberNames.Select(item => $"Input.{nameof(GeneticAlgorithmParameters)}.{item}"));
                    }
                }
            }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public string NetworkId { get; set; }

            public string NetworkName { get; set; }

            public bool HasNetworkDatabases { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string networkId, string analysisId, bool loadDemonstration)
        {
            // Check if the demonstration should be loaded.
            if (loadDemonstration)
            {
                // Check if there are no demonstration items configured.
                if (string.IsNullOrEmpty(_configuration["Data:Demonstration:AnalysisId"]))
                {
                    // Try to get a demonstration control path.
                    var controlPath = _context.ControlPaths
                        .Include(item => item.Analysis)
                            .ThenInclude(item => item.Network)
                        .Where(item => item.Analysis.IsPublic && item.Analysis.IsDemonstration && item.Analysis.Network.IsPublic && item.Analysis.Network.IsDemonstration)
                        .AsNoTracking()
                        .FirstOrDefault();
                    // Check if there was no demonstration control path found.
                    if (controlPath == null || controlPath.Analysis == null || controlPath.Analysis.Network == null)
                    {
                        // Display a message.
                        TempData["StatusMessage"] = "Error: There are no demonstration analyses available.";
                        // Redirect to the index page.
                        return RedirectToPage("/CreatedData/Analyses/Index");
                    }
                    // Update the demonstration item IDs.
                    _configuration["Data:Demonstration:NetworkId"] = controlPath.Analysis.Network.Id;
                    _configuration["Data:Demonstration:AnalysisId"] = controlPath.Analysis.Id;
                    _configuration["Data:Demonstration:ControlPathId"] = controlPath.Id;
                }
                // Get the IDs of the configured demonstration item.
                networkId = _configuration["Data:Demonstration:NetworkId"];
                analysisId = _configuration["Data:Demonstration:AnalysisId"];
            }
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the view.
            View = new ViewModel { };
            // Check if there was an analysis provided.
            if (!string.IsNullOrEmpty(analysisId))
            {
                // Try to get the analysis with the provided ID.
                var analyses = _context.Analyses
                    .Where(item => item.IsPublic || (user != null && item.AnalysisUsers.Any(item1 => item1.Email == user.Email)))
                    .Where(item => item.Id == analysisId);
                // Check if there was an ID provided, but there was no analysis found.
                if (analyses == null || !analyses.Any())
                {
                    // Display a message.
                    TempData["StatusMessage"] = "Error: The specified analysis could not be found, or you don't have access to it.";
                    // Redirect to the index page.
                    return RedirectToPage("/CreatedData/Analyses/Index");
                }
                // Get the network item.
                var analysisNetworkItem = analyses
                    .Select(item => item.Network)
                    .Where(item => item.IsPublic || (user != null && item.NetworkUsers.Any(item1 => item1.Email == user.Email)))
                    .Select(item => new
                    {
                        Id = item.Id,
                        Name = item.Name,
                        HasNetworkDatabases = item.NetworkDatabases.Any()
                    })
                    .FirstOrDefault();
                // Check if there wasn't any network found.
                if (analysisNetworkItem == null)
                {
                    // Display a message.
                    TempData["StatusMessage"] = "Error: The network corresponding to the specified analysis could not be found, or you do not have access to it.";
                    // Redirect to the index page.
                    return RedirectToPage("/CreatedData/Analyses/Index");
                }
                // Update the view.
                View.NetworkId = analysisNetworkItem.Id;
                View.NetworkName = analysisNetworkItem.Name;
                View.HasNetworkDatabases = analysisNetworkItem.HasNetworkDatabases;
                // Get the related data.
                var sourceProteinNames = _context.AnalysisProteins
                    .Where(item => item.Type == AnalysisProteinType.Source && item.Analysis.Id == analysisId)
                    .Select(item => item.Protein.Name)
                    .AsNoTracking()
                    .ToList();
                var sourceProteinCollectionIds = _context.AnalysisProteinCollections
                    .Where(item => item.Type == AnalysisProteinCollectionType.Source && item.Analysis.Id == analysisId)
                    .Select(item => item.ProteinCollection.Id)
                    .AsNoTracking()
                    .ToList();
                var targetProteinNames = _context.AnalysisProteins
                    .Where(item => item.Type == AnalysisProteinType.Target && item.Analysis.Id == analysisId)
                    .Select(item => item.Protein.Name)
                    .AsNoTracking()
                    .ToList();
                var targetProteinCollectionIds = _context.AnalysisProteinCollections
                    .Where(item => item.Type == AnalysisProteinCollectionType.Target && item.Analysis.Id == analysisId)
                    .Select(item => item.ProteinCollection.Id)
                    .AsNoTracking()
                    .ToList();
                // Define the input.
                Input = new InputModel
                {
                    Name = analyses
                        .Select(item => item.Name)
                        .FirstOrDefault(),
                    Description = analyses
                        .Select(item => item.Description)
                        .FirstOrDefault(),
                    IsPublic = user == null,
                    NetworkId = View.NetworkId,
                    UseSourceProteinData = sourceProteinNames != null && sourceProteinNames.Any(),
                    SourceProteinData = JsonSerializer.Serialize(sourceProteinNames),
                    UseSourceProteinCollectionData = sourceProteinCollectionIds != null && sourceProteinCollectionIds.Any(),
                    SourceProteinCollectionData = JsonSerializer.Serialize(sourceProteinCollectionIds),
                    UseTargetProteinData = targetProteinNames != null && targetProteinNames.Any(),
                    TargetProteinData = JsonSerializer.Serialize(targetProteinNames),
                    UseTargetProteinCollectionData = targetProteinCollectionIds != null && targetProteinCollectionIds.Any(),
                    TargetProteinCollectionData = JsonSerializer.Serialize(targetProteinCollectionIds),
                    MaximumIterations = analyses
                        .Select(item => item.MaximumIterations)
                        .FirstOrDefault(),
                    MaximumIterationsWithoutImprovement = analyses
                        .Select(item => item.MaximumIterationsWithoutImprovement)
                        .FirstOrDefault(),
                    Algorithm = analyses
                        .Select(item => item.Algorithm)
                        .FirstOrDefault()
                        .ToString(),
                    UseDefaultAlgorithmParameters = false
                };
                // Update the parameters.
                Input.GreedyAlgorithmParameters = Input.Algorithm == AnalysisAlgorithm.Greedy.ToString() ? JsonSerializer.Deserialize<GreedyAlgorithm.Parameters>(analyses.Select(item => item.Parameters).FirstOrDefault()) : new GreedyAlgorithm.Parameters();
                Input.GeneticAlgorithmParameters = Input.Algorithm == AnalysisAlgorithm.Genetic.ToString() ? JsonSerializer.Deserialize<GeneticAlgorithm.Parameters>(analyses.Select(item => item.Parameters).FirstOrDefault()) : new GeneticAlgorithm.Parameters();
                // Display a message.
                TempData["StatusMessage"] = "Success: The analysis has been loaded successfully.";
                // Return the page.
                return Page();
            }
            // Check if there wasn't a network provided.
            if (string.IsNullOrEmpty(networkId))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A network is required to create an analysis.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Get the network item.
            var networkItem = _context.Networks
                .Where(item => item.IsPublic || (user != null && item.NetworkUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == networkId)
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    HasNetworkDatabases = item.NetworkDatabases.Any()
                })
                .FirstOrDefault();
            // Check if there wasn't any network found.
            if (networkItem == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The specified network could not be found, or you do not have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Update the view.
            View.NetworkId = networkItem.Id;
            View.NetworkName = networkItem.Name;
            View.HasNetworkDatabases = networkItem.HasNetworkDatabases;
            // Define the input.
            Input = new InputModel
            {
                IsPublic = user == null,
                NetworkId = View.NetworkId,
                UseSourceProteinData = false,
                SourceProteinData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                UseSourceProteinCollectionData = false,
                SourceProteinCollectionData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                UseTargetProteinData = View.HasNetworkDatabases ? false : true,
                TargetProteinData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                UseTargetProteinCollectionData = View.HasNetworkDatabases ? true : false,
                TargetProteinCollectionData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                MaximumIterations = 100,
                MaximumIterationsWithoutImprovement = 25,
                Algorithm = AnalysisAlgorithm.Greedy.ToString(),
                UseDefaultAlgorithmParameters = true,
                GreedyAlgorithmParameters = new GreedyAlgorithm.Parameters(),
                GeneticAlgorithmParameters = new GeneticAlgorithm.Parameters()
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there wasn't a network provided.
            if (string.IsNullOrEmpty(Input.NetworkId))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A network is required to create an analysis.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Get the network item.
            var networkItem = _context.Networks
                .Where(item => item.IsPublic || (user != null && item.NetworkUsers.Any(item1 => item1.Email == user.Email)))
                .Where(item => item.Id == Input.NetworkId)
                .Select(item => new
                {
                    Id = item.Id,
                    Name = item.Name,
                    HasNetworkDatabases = item.NetworkDatabases.Any()
                })
                .FirstOrDefault();
            // Check if there wasn't any network found.
            if (networkItem == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: The specified network could not be found, or you do not have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                NetworkId = networkItem.Id,
                NetworkName = networkItem.Name,
                HasNetworkDatabases = networkItem.HasNetworkDatabases
            };
            // Check if the reCaptcha is valid.
            if (!await _reCaptchaChecker.IsValid(Input.ReCaptchaToken))
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The reCaptcha verification failed.");
                // Return the page.
                return Page();
            }
            // Check if the default parameters should be used.
            if (Input.UseDefaultAlgorithmParameters)
            {
                // Update the parameters.
                Input.MaximumIterations = 100;
                Input.MaximumIterationsWithoutImprovement = 25;
                Input.GreedyAlgorithmParameters = new GreedyAlgorithm.Parameters();
                Input.GeneticAlgorithmParameters = new GeneticAlgorithm.Parameters();
                // Clear the current model state.
                ModelState.Clear();
                // Revalidate the model state.
                TryValidateModel(Input);
            }
            // Check if the provided model isn't valid.
            if (!ModelState.IsValid)
            {
                // Get the validation errors.
                var validationErrors = ModelState.Values.Select(item => item.Errors).SelectMany(item => item).ToList();
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "An error has been encountered. Please check again the input fields.");
                // Go over each validation error.
                foreach (var validationError in validationErrors)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, validationError.ErrorMessage);
                }
                // Redisplay the page.
                return Page();
            }
            // Check if the public availability isn't valid.
            if (user == null && !Input.IsPublic)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "You are not logged in, so the analysis must be set as public.");
                // Redisplay the page.
                return Page();
            }
            // Check if no target data providing method was selected.
            if (!Input.UseTargetProteinData && !Input.UseTargetProteinCollectionData)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "At least one method for providing target proteins needs to be selected.");
                // Redisplay the page.
                return Page();
            }
            // Try to get the algorithm.
            try
            {
                // Get the algorithm.
                var algorithm = EnumerationExtensions.GetEnumerationValue<AnalysisAlgorithm>(Input.Algorithm);
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Define the related data.
            var sourceProteinIdentifiers = new List<string>();
            var sourceProteinCollectionIds = new List<string>();
            // Check if source proteins should be used.
            if (Input.UseSourceProteinData)
            {
                // Try to deserialize the source data.
                if (!Input.SourceProteinData.TryDeserializeJsonObject<List<string>>(out sourceProteinIdentifiers) || sourceProteinIdentifiers == null)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided source data could not be deserialized.");
                    // Redisplay the page.
                    return Page();
                }
            }
            // Check if source protein collection data should be used.
            if (Input.UseSourceProteinCollectionData)
            {
                // Try to deserialize the source protein collection data.
                if (!Input.SourceProteinCollectionData.TryDeserializeJsonObject<List<string>>(out sourceProteinCollectionIds) || sourceProteinCollectionIds == null)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided source protein collection data could not be deserialized.");
                    // Redisplay the page.
                    return Page();
                }
                // Keep only the valid IDs.
                sourceProteinCollectionIds = _context.ProteinCollections
                    .Where(item => item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Source))
                    .Where(item => sourceProteinCollectionIds.Contains(item.Id))
                    .Select(item => item.Id)
                    .ToList();
            }
            // Define the related data.
            var targetProteinIdentifiers = new List<string>();
            var targetProteinCollectionIds = new List<string>();
            // Check if target proteins should be used.
            if (Input.UseTargetProteinData)
            {
                // Try to deserialize the target data.
                if (!Input.TargetProteinData.TryDeserializeJsonObject<List<string>>(out targetProteinIdentifiers) || targetProteinIdentifiers == null)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided target data could not be deserialized.");
                    // Redisplay the page.
                    return Page();
                }
            }
            // Check if target protein collection data should be used.
            if (Input.UseTargetProteinCollectionData)
            {
                // Try to deserialize the target protein collection data.
                if (!Input.TargetProteinCollectionData.TryDeserializeJsonObject<List<string>>(out targetProteinCollectionIds) || targetProteinCollectionIds == null)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided target protein collection data could not be deserialized.");
                    // Redisplay the page.
                    return Page();
                }
                // Keep only the valid IDs.
                targetProteinCollectionIds = _context.ProteinCollections
                    .Where(item => item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Target))
                    .Where(item => targetProteinCollectionIds.Contains(item.Id))
                    .Select(item => item.Id)
                    .ToList();
            }
            // Check if there weren't any target items found.
            if (!targetProteinIdentifiers.Any() && !targetProteinCollectionIds.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No target proteins could be found within the provided target data or the selected target protein collections.");
                // Redisplay the page.
                return Page();
            }
            // Serialize the seed data.
            var data = JsonSerializer.Serialize(sourceProteinIdentifiers
                .Select(item => new AnalysisProteinInputModel
                {
                    Protein = new ProteinInputModel
                    {
                        Id = item
                    },
                    Type = "Source"
                })
                .Concat(targetProteinIdentifiers
                    .Select(item => new AnalysisProteinInputModel
                    {
                        Protein = new ProteinInputModel
                        {
                            Id = item
                        },
                        Type = "Target"
                    })));
            // Define a new task.
            var task = new AnalysesTask
            {
                Scheme = HttpContext.Request.Scheme,
                HostValue = HttpContext.Request.Host.Value,
                Items = new List<AnalysisInputModel>
                {
                    new AnalysisInputModel
                    {
                        Name = Input.Name,
                        Description = Input.Description,
                        IsPublic = Input.IsPublic,
                        Data = data,
                        MaximumIterations = Input.MaximumIterations,
                        MaximumIterationsWithoutImprovement = Input.MaximumIterationsWithoutImprovement,
                        Algorithm = Input.Algorithm,
                        Parameters = Input.Algorithm == AnalysisAlgorithm.Greedy.ToString() ? JsonSerializer.Serialize(Input.GreedyAlgorithmParameters) :
                            Input.Algorithm == AnalysisAlgorithm.Genetic.ToString() ? JsonSerializer.Serialize(Input.GeneticAlgorithmParameters) :
                            null,
                        Network = new NetworkInputModel
                        {
                            Id = Input.NetworkId
                        },
                        AnalysisUsers = user != null ?
                            new List<AnalysisUserInputModel>
                            {
                                new AnalysisUserInputModel
                                {
                                    User = new UserInputModel
                                    {
                                        Id = user.Id
                                    },
                                    Email = user.Email
                                }
                            } :
                            new List<AnalysisUserInputModel>(),
                        AnalysisProteinCollections = sourceProteinCollectionIds
                            .Select(item => new AnalysisProteinCollectionInputModel
                            {
                                ProteinCollection = new ProteinCollectionInputModel
                                {
                                    Id = item
                                },
                                Type = "Source"
                            })
                            .Concat(targetProteinCollectionIds
                                .Select(item => new AnalysisProteinCollectionInputModel
                                {
                                    ProteinCollection = new ProteinCollectionInputModel
                                    {
                                        Id = item
                                    },
                                    Type = "Target"
                                }))
                    }
                }
            };
            // Define the IDs of the created items.
            var ids = Enumerable.Empty<string>();
            // Try to run the task.
            try
            {
                // Run the task.
                ids = await task.CreateAsync(_serviceProvider, CancellationToken.None);
            }
            catch (Exception exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, exception.Message);
                // Redisplay the page.
                return Page();
            }
            // Check if there wasn't any ID returned.
            if (ids == null || !ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = $"Success: 1 analysis defined successfully and scheduled for generation.";
                // Redirect to the index page.
                return RedirectToPage("/CreatedData/Analyses/Index");
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 analysis defined successfully with the ID \"{ids.First()}\" and scheduled for generation.";
            // Redirect to the index page.
            return RedirectToPage("/CreatedData/Analyses/Details/Index", new { id = ids.First() });
        }

        public IActionResult OnGetSourceProteinCollections(DataTableParametersViewModel parameters)
        {
            // Start with all of the items to which the user has access.
            var query = _context.ProteinCollections
                .Where(item => item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Source));
            // Get the total count.
            var totalCount = query.Count();
            // Check if there is search applied.
            if (parameters.Search != null && !string.IsNullOrEmpty(parameters.Search.Value))
            {
                // Select the results matching the search string.
                query = query
                    .Where(item => item.Id.Contains(parameters.Search.Value) || item.Name.Contains(parameters.Search.Value));
            }
            // Check if there is sorting applied.
            if (parameters.Order != null && parameters.Order.Any())
            {
                // Get the sorting column and direction.
                var column = parameters.Columns.ElementAtOrDefault(parameters.Order.FirstOrDefault()?.Column ?? -1)?.Name;
                var direction = parameters.Order.FirstOrDefault()?.Direction;
                // Switch based on the ordering parameters.
                switch ((column, direction))
                {
                    case var sort when sort == ("IsSelected", "Ascending"):
                        query = query.OrderBy(item => parameters.SelectedItems.Contains(item.Id));
                        break;
                    case var sort when sort == ("IsSelected", "Descending"):
                        query = query.OrderByDescending(item => parameters.SelectedItems.Contains(item.Id));
                        break;
                    case var sort when sort == ("Name", "Ascending"):
                        query = query.OrderBy(item => item.Name);
                        break;
                    case var sort when sort == ("Name", "Descending"):
                        query = query.OrderByDescending(item => item.Name);
                        break;
                    default:
                        break;
                }
            }
            // Get the filtered count.
            var filteredCount = query.Count();
            // Take only the results on the current page.
            query = query
                .Skip(parameters.Start)
                .Take(parameters.Length)
                .AsNoTracking();
            // Return the JSON response.
            return new JsonResult(new DataTableResponseViewModel
            {
                Data = query
                    .Select(item => new List<string>
                    {
                        item.Id,
                        item.Name,
                        parameters.SelectedItems.Contains(item.Id).ToString()
                    })
                    .ToList(),
                Draw = parameters.Draw,
                RecordsFiltered = filteredCount,
                RecordsTotal = totalCount
            });
        }

        public IActionResult OnGetTargetProteinCollections(DataTableParametersViewModel parameters)
        {
            // Start with all of the items to which the user has access.
            var query = _context.ProteinCollections
                .Where(item => item.ProteinCollectionTypes.Any(item1 => item1.Type == EnumerationProteinCollectionType.Target));
            // Get the total count.
            var totalCount = query.Count();
            // Check if there is search applied.
            if (parameters.Search != null && !string.IsNullOrEmpty(parameters.Search.Value))
            {
                // Select the results matching the search string.
                query = query
                    .Where(item => item.Id.Contains(parameters.Search.Value) || item.Name.Contains(parameters.Search.Value));
            }
            // Check if there is sorting applied.
            if (parameters.Order != null && parameters.Order.Any())
            {
                // Get the sorting column and direction.
                var column = parameters.Columns.ElementAtOrDefault(parameters.Order.FirstOrDefault()?.Column ?? -1)?.Name;
                var direction = parameters.Order.FirstOrDefault()?.Direction;
                // Switch based on the ordering parameters.
                switch ((column, direction))
                {
                    case var sort when sort == ("IsSelected", "Ascending"):
                        query = query.OrderBy(item => parameters.SelectedItems.Contains(item.Id));
                        break;
                    case var sort when sort == ("IsSelected", "Descending"):
                        query = query.OrderByDescending(item => parameters.SelectedItems.Contains(item.Id));
                        break;
                    case var sort when sort == ("Name", "Ascending"):
                        query = query.OrderBy(item => item.Name);
                        break;
                    case var sort when sort == ("Name", "Descending"):
                        query = query.OrderByDescending(item => item.Name);
                        break;
                    default:
                        break;
                }
            }
            // Get the filtered count.
            var filteredCount = query.Count();
            // Take only the results on the current page.
            query = query
                .Skip(parameters.Start)
                .Take(parameters.Length)
                .AsNoTracking();
            // Return the JSON response.
            return new JsonResult(new DataTableResponseViewModel
            {
                Data = query
                    .Select(item => new List<string>
                    {
                        item.Id,
                        item.Name,
                        parameters.SelectedItems.Contains(item.Id).ToString()
                    })
                    .ToList(),
                Draw = parameters.Draw,
                RecordsFiltered = filteredCount,
                RecordsTotal = totalCount
            });
        }
    }
}
