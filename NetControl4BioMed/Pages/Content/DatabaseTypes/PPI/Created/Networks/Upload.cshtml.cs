using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using NetControl4BioMed.Helpers.InputModels;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.Tasks;

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.PPI.Created.Networks
{
    [RequestFormLimits(ValueLengthLimit = 16 * 1024 * 1024)]
    public class UploadModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public UploadModel(IServiceProvider serviceProvider, UserManager<User> userManager, ApplicationDbContext context, IReCaptchaChecker reCaptchaChecker)
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
            public string Name { get; set; }

            [DataType(DataType.MultilineText)]
            public string Description { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public bool IsPublic { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string NodeDatabaseData { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string EdgeDatabaseData { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string SeedData { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public bool IsUserAuthenticated { get; set; }

            public IEnumerable<Sample> Samples { get; set; }

            public IEnumerable<Database> NodeDatabases { get; set; }

            public IEnumerable<Database> EdgeDatabases { get; set; }
        }

        public class ItemModel
        {
            public string SourceNode { get; set; }

            public string TargetNode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string networkId = null, string sampleId = null)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Try to get the network with the provided ID.
            var networks = _context.Networks
                .Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI"))
                .Where(item => item.IsPublic || item.NetworkUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == networkId);
            // Check if there was an ID provided, but there was no network found.
            if (!string.IsNullOrEmpty(networkId) && (networks == null || !networks.Any()))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No network could be found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                Samples = _context.Samples
                    .Where(item => item.SampleDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI")),
                NodeDatabases = _context.Databases
                    .Where(item => item.DatabaseType.Name == "PPI")
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                    .Where(item => item.DatabaseNodeFields.Any(item1 => item1.IsSearchable)),
                EdgeDatabases = _context.Databases
                    .Where(item => item.DatabaseType.Name == "PPI")
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                    .Where(item => item.DatabaseEdges.Any())
            };
            // Check if there weren't any node databases available.
            if (View.NodeDatabases == null || !View.NodeDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no protein databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Networks/Index");
            }
            // Check if there weren't any edge databases available.
            if (View.EdgeDatabases == null || !View.EdgeDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no interaction databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Networks/Index");
            }
            // Try to get the sample with the provided ID.
            var sample = View.Samples?
                .FirstOrDefault(item => item.Id == sampleId);
            // Check if there was an ID provided, but there was no sample found.
            if (!string.IsNullOrEmpty(sampleId) && sample == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No sample could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Networks/Index");
            }
            // Check if there was a network provided.
            if (!string.IsNullOrEmpty(networkId))
            {
                // Define the input.
                Input = new InputModel
                {
                    Name = networks
                        .Select(item => item.Name)
                        .FirstOrDefault(),
                    Description = networks
                        .Select(item => item.Description)
                        .FirstOrDefault(),
                    IsPublic = !View.IsUserAuthenticated,
                    NodeDatabaseData = JsonSerializer.Serialize(networks
                        .Select(item => item.NetworkDatabases)
                        .SelectMany(item => item)
                        .Select(item => item.Database)
                        .Intersect(View.NodeDatabases)
                        .Select(item => item.Id)),
                    EdgeDatabaseData = JsonSerializer.Serialize(networks
                        .Select(item => item.NetworkDatabases)
                        .SelectMany(item => item)
                        .Select(item => item.Database)
                        .Intersect(View.EdgeDatabases)
                        .Select(item => item.Id)),
                    SeedData = JsonSerializer.Serialize(networks
                        .Select(item => item.NetworkEdges)
                        .SelectMany(item => item)
                        .Select(item => new ItemModel
                        {
                            SourceNode = item.Edge.EdgeNodes
                                .Where(item1 => item1.Type == EdgeNodeType.Source)
                                .Select(item1 => item1.Node.Name)
                                .FirstOrDefault(),
                            TargetNode = item.Edge.EdgeNodes
                                .Where(item1 => item1.Type == EdgeNodeType.Target)
                                .Select(item1 => item1.Node.Name)
                                .FirstOrDefault()
                        })
                        .Where(item => !string.IsNullOrEmpty(item.SourceNode) && !string.IsNullOrEmpty(item.TargetNode)))
                };
            }
            // Check if there was a sample provided.
            else if (!string.IsNullOrEmpty(sampleId))
            {
                // Define the input.
                Input = new InputModel
                {
                    Name = sample.NetworkName,
                    Description = sample.NetworkDescription,
                    IsPublic = !View.IsUserAuthenticated,
                    NodeDatabaseData = sample.NetworkNodeDatabaseData,
                    EdgeDatabaseData = sample.NetworkEdgeDatabaseData,
                    SeedData = sample.NetworkSeedData
                };
            }
            else
            {
                // Define the input.
                Input = new InputModel
                {
                    IsPublic = !View.IsUserAuthenticated,
                    NodeDatabaseData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                    EdgeDatabaseData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                    SeedData = JsonSerializer.Serialize(Enumerable.Empty<ItemModel>())
                };
            }
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                Samples = _context.Samples
                    .Where(item => item.SampleDatabases.Any(item1 => item1.Database.DatabaseType.Name == "PPI")),
                NodeDatabases = _context.Databases
                    .Where(item => item.DatabaseType.Name == "PPI")
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                    .Where(item => item.DatabaseNodeFields.Any(item1 => item1.IsSearchable)),
                EdgeDatabases = _context.Databases
                    .Where(item => item.DatabaseType.Name == "PPI")
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                    .Where(item => item.DatabaseEdges.Any())
            };
            // Check if there weren't any node databases available.
            if (View.NodeDatabases == null || !View.NodeDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no protein databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Networks/Index");
            }
            // Check if there weren't any edge databases available.
            if (View.EdgeDatabases == null || !View.EdgeDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no interaction databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Networks/Index");
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
            // Check if the public availability isn't valid.
            if (!View.IsUserAuthenticated && !Input.IsPublic)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "You are not logged in, so the network must be set as public.");
                // Redisplay the page.
                return Page();
            }
            // Try to deserialize the node database data.
            if (!Input.NodeDatabaseData.TryDeserializeJsonObject<IEnumerable<string>>(out var nodeDatabaseIds) || nodeDatabaseIds == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided protein database data could not be deserialized.");
                // Redisplay the page.
                return Page();
            }
            // Check if there weren't any node database IDs provided.
            if (!nodeDatabaseIds.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "At least one protein database ID must be provided.");
                // Redisplay the page.
                return Page();
            }
            // Try to get the node databases with the provided IDs.
            var nodeDatabases = View.NodeDatabases.Where(item => nodeDatabaseIds.Contains(item.Id));
            // Check if there weren't any node databases found.
            if (!nodeDatabases.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No protein databases could be found with the provided ID(s).");
                // Redisplay the page.
                return Page();
            }
            // Try to deserialize the edge database data.
            if (!Input.EdgeDatabaseData.TryDeserializeJsonObject<IEnumerable<string>>(out var edgeDatabaseIds) || edgeDatabaseIds == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided interaction database data could not be deserialized.");
                // Redisplay the page.
                return Page();
            }
            // Check if there weren't any edge database IDs provided.
            if (!edgeDatabaseIds.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "At least one interaction database ID must be provided.");
                // Redisplay the page.
                return Page();
            }
            // Try to get the edge databases with the provided IDs.
            var edgeDatabases = View.EdgeDatabases.Where(item => edgeDatabaseIds.Contains(item.Id));
            // Check if there weren't any edge databases found.
            if (!edgeDatabases.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No interaction databases could be found with the provided ID(s).");
                // Redisplay the page.
                return Page();
            }
            // Try to deserialize the seed data.
            if (!Input.SeedData.TryDeserializeJsonObject<IEnumerable<ItemModel>>(out var items) || items == null)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided seed data could not be deserialized.");
                // Redisplay the page.
                return Page();
            }
            // Check if there weren't any items found.
            if (!items.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No interactions could be found within the provided seed data.");
                // Redisplay the page.
                return Page();
            }
            // Serialize the seed data.
            var data = JsonSerializer.Serialize(items
                .Select(item => new NetworkEdgeInputModel
                {
                    Edge = new EdgeInputModel
                    {
                        EdgeNodes = new List<EdgeNodeInputModel>
                        {
                                new EdgeNodeInputModel
                                {
                                    Node = new NodeInputModel
                                    {
                                        Id = item.SourceNode
                                    },
                                    Type = "Source"
                                },
                                new EdgeNodeInputModel
                                {
                                    Node = new NodeInputModel
                                    {
                                        Id = item.TargetNode
                                    },
                                    Type = "Target"
                                }
                        }
                    }
                }));
            // Define a new task.
            var task = new NetworksTask
            {
                Scheme = HttpContext.Request.Scheme,
                HostValue = HttpContext.Request.Host.Value,
                Items = new List<NetworkInputModel>
                {
                    new NetworkInputModel
                    {
                        Name = Input.Name,
                        Description = Input.Description,
                        IsPublic = Input.IsPublic,
                        Algorithm = NetworkAlgorithm.None.ToString(),
                        Data = data,
                        NetworkDatabases = nodeDatabases
                            .Select(item => item.Id)
                            .Select(item => new NetworkDatabaseInputModel
                            {
                                Database = new DatabaseInputModel
                                {
                                    Id = item
                                },
                                Type = "Node"
                            })
                            .Concat(edgeDatabases
                                .Select(item => item.Id)
                                .Select(item => new NetworkDatabaseInputModel
                                {
                                    Database = new DatabaseInputModel
                                    {
                                        Id = item
                                    },
                                    Type = "Edge"
                                })),
                        NetworkUsers = View.IsUserAuthenticated ?
                            new List<NetworkUserInputModel>
                            {
                                new NetworkUserInputModel
                                {
                                    User = new UserInputModel
                                    {
                                        Id = user.Id
                                    }
                                }
                            } :
                            new List<NetworkUserInputModel>()
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
            if (ids != null && ids.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = $"Success: 1 PPI network defined successfully with the ID \"{ids.First()}\" and scheduled for generation.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Networks/Details/Index", new { id = ids.First() });
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 PPI network of type defined successfully and scheduled for generation.";
            // Redirect to the index page.
            return RedirectToPage("/Content/DatabaseTypes/PPI/Created/Networks/Index");
        }
    }
}
