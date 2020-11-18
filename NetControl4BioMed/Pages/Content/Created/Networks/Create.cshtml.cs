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

namespace NetControl4BioMed.Pages.Content.Created.Networks
{
    [RequestFormLimits(ValueLengthLimit = 16 * 1024 * 1024)]
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
            public bool IsPublic { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string Algorithm { get; set; }

            [DataType(DataType.MultilineText)]
            [Required(ErrorMessage = "This field is required.")]
            public string SeedData { get; set; }

            public IEnumerable<string> NodeDatabaseIds { get; set; }

            public IEnumerable<string> EdgeDatabaseIds { get; set; }

            public IEnumerable<string> SeedNodeCollectionIds { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public bool IsUserAuthenticated { get; set; }

            public bool IsGeneric { get; set; }

            public IEnumerable<Database> NodeDatabases { get; set; }

            public IEnumerable<Database> EdgeDatabases { get; set; }

            public IEnumerable<NodeCollection> SeedNodeCollections { get; set; }
        }

        public class ItemModel
        {
            public string SourceNode { get; set; }

            public string TargetNode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string databaseTypeId = null, string networkId = null)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Check if there wasn't any database type ID provided.
            if (string.IsNullOrEmpty(databaseTypeId))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A type is required for creating a network.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
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
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Check if the database type is generic.
            var isGeneric = databaseType.Name == "Generic";
            // Try to get the network with the provided ID.
            var networks = _context.Networks
                .Where(item => item.IsPublic || item.NetworkUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == networkId);
            // Check if there wasn't any network found.
            var networksFound = networks != null && networks.Any();
            // Check if there was an ID provided, but there was no network found.
            if (!string.IsNullOrEmpty(networkId) && !networksFound)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No network could be found with the provided ID, or you don't have access to it.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                IsGeneric = isGeneric,
                NodeDatabases = _context.Databases
                    .Where(item => item.DatabaseType == databaseType)
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                    .Where(item => !isGeneric ? item.DatabaseNodeFields.Any(item1 => item1.IsSearchable) : true),
                EdgeDatabases = _context.Databases
                    .Where(item => item.DatabaseType == databaseType)
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                    .Where(item => !isGeneric ? item.DatabaseEdges.Any() : true),
                SeedNodeCollections = _context.NodeCollections
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType == databaseType))
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
            };
            // Check if there weren't any node databases available.
            if (!View.NodeDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no node databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Check if there weren't any edge databases available.
            if (!View.EdgeDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no edge databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the input based on the database type and the provided networks.
            switch ((networksFound, View.IsGeneric))
            {
                case (false, false):
                    Input = new InputModel
                    {
                        IsPublic = !View.IsUserAuthenticated,
                        DatabaseTypeId = databaseType.Id,
                        SeedData = JsonSerializer.Serialize(Enumerable.Empty<string>()),
                        NodeDatabaseIds = Enumerable.Empty<string>(),
                        EdgeDatabaseIds = Enumerable.Empty<string>()
                    };
                    break;
                case (false, true):
                    Input = new InputModel
                    {
                        IsPublic = !View.IsUserAuthenticated,
                        DatabaseTypeId = databaseType.Id,
                        SeedData = JsonSerializer.Serialize(Enumerable.Empty<ItemModel>()),
                        NodeDatabaseIds = View.NodeDatabases.Select(item => item.Id),
                        EdgeDatabaseIds = View.EdgeDatabases.Select(item => item.Id)
                    };
                    break;
                case (true, false):
                    Input = new InputModel
                    {
                        IsPublic = networks
                            .Select(item => item.IsPublic)
                            .FirstOrDefault(),
                        DatabaseTypeId = databaseType.Id,
                        Name = networks
                            .Select(item => item.Name)
                            .FirstOrDefault(),
                        Description = networks
                            .Select(item => item.Description)
                            .FirstOrDefault(),
                        Algorithm = networks
                            .Select(item => item.Algorithm)
                            .FirstOrDefault()
                            .ToString(),
                        SeedData = JsonSerializer.Serialize(networks
                            .Select(item => item.NetworkNodes)
                            .SelectMany(item => item)
                            .Where(item => item.Type == NetworkNodeType.Seed)
                            .Select(item => item.Node.Name)),
                        NodeDatabaseIds = networks
                            .Select(item => item.NetworkDatabases)
                            .SelectMany(item => item)
                            .Select(item => item.Database)
                            .Intersect(View.NodeDatabases)
                            .Select(item => item.Id),
                        EdgeDatabaseIds = networks
                            .Select(item => item.NetworkDatabases)
                            .SelectMany(item => item)
                            .Select(item => item.Database)
                            .Intersect(View.EdgeDatabases)
                            .Select(item => item.Id),
                        SeedNodeCollectionIds = networks
                            .Select(item => item.NetworkNodeCollections)
                            .SelectMany(item => item)
                            .Select(item => item.NodeCollection)
                            .Intersect(View.SeedNodeCollections)
                            .Select(item => item.Id)
                    };
                    break;
                case (true, true):
                    Input = new InputModel
                    {
                        IsPublic = networks
                            .Select(item => item.IsPublic)
                            .FirstOrDefault(),
                        DatabaseTypeId = databaseType.Id,
                        Name = networks
                            .Select(item => item.Name)
                            .FirstOrDefault(),
                        Description = networks
                            .Select(item => item.Description)
                            .FirstOrDefault(),
                        Algorithm = networks
                            .Select(item => item.Algorithm)
                            .FirstOrDefault()
                            .ToString(),
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
                            .Where(item => !string.IsNullOrEmpty(item.SourceNode) && !string.IsNullOrEmpty(item.TargetNode))),
                        NodeDatabaseIds = networks
                            .Select(item => item.NetworkDatabases)
                            .SelectMany(item => item)
                            .Select(item => item.Database)
                            .Intersect(View.NodeDatabases)
                            .Select(item => item.Id),
                        EdgeDatabaseIds = networks
                            .Select(item => item.NetworkDatabases)
                            .SelectMany(item => item)
                            .Select(item => item.Database)
                            .Intersect(View.EdgeDatabases)
                            .Select(item => item.Id),
                        SeedNodeCollectionIds = networks
                            .Select(item => item.NetworkNodeCollections)
                            .SelectMany(item => item)
                            .Select(item => item.NodeCollection)
                            .Intersect(View.SeedNodeCollections)
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
            // Check if there isn't any database type ID provided or if the database type ID couldn't be inferred.
            if (string.IsNullOrEmpty(Input.DatabaseTypeId))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A type is required for creating an analysis.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Try to get the database type with the provided ID.
            var databaseType = _context.DatabaseTypes.FirstOrDefault(item => item.Id == Input.DatabaseTypeId);
            // Check if there wasn't any database type found.
            if (databaseType == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No type could be found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Check if the database type is generic.
            var isGeneric = databaseType.Name == "Generic";
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                IsGeneric = isGeneric,
                NodeDatabases = _context.Databases
                    .Where(item => item.DatabaseType == databaseType)
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                    .Where(item => isGeneric || item.DatabaseNodeFields.Any(item1 => item1.IsSearchable)),
                EdgeDatabases = _context.Databases
                    .Where(item => item.DatabaseType == databaseType)
                    .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user))
                    .Where(item => isGeneric || item.DatabaseEdges.Any()),
                SeedNodeCollections = _context.NodeCollections
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.DatabaseType == databaseType))
                    .Where(item => item.NodeCollectionDatabases.Any(item1 => item1.Database.IsPublic || item1.Database.DatabaseUsers.Any(item2 => item2.User == user)))
            };
            // Check if there weren't any node databases available.
            if (!View.NodeDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no node databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Check if there weren't any edge databases available.
            if (!View.EdgeDatabases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no edge databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
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
            // Try to get the algorithm.
            try
            {
                // Get the algorithm.
                var algorithm = EnumerationExtensions.GetEnumerationValue<NetworkAlgorithm>(Input.Algorithm);
            }
            catch (Exception)
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The network generation algorithm couldn't be determined from the provided string.");
                // Redisplay the page.
                return Page();
            }
            // Check if the algorithm is not valid.
            if (View.IsGeneric ^ Input.Algorithm == "None")
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The network generation algorithm is not valid for the provided database type.");
                // Redisplay the page.
                return Page();
            }
            // Get the provided node database IDs.
            var nodeDatabaseIds = Input.NodeDatabaseIds ?? Enumerable.Empty<string>();
            // Check if there weren't any node database IDs provided.
            if (!nodeDatabaseIds.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "At least one node database must be selected.");
                // Redisplay the page.
                return Page();
            }
            // Try to get the node databases with the provided IDs.
            var nodeDatabases = View.NodeDatabases.Where(item => nodeDatabaseIds.Contains(item.Id));
            // Check if there weren't any node databases found.
            if (!nodeDatabases.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No node databases could be found with the provided IDs.");
                // Redisplay the page.
                return Page();
            }
            // Get the provided edge database IDs.
            var edgeDatabaseIds = Input.EdgeDatabaseIds ?? Enumerable.Empty<string>();
            // Check if there weren't any edge database IDs provided.
            if (!edgeDatabaseIds.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "At least one edge database must be selected.");
                // Redisplay the page.
                return Page();
            }
            // Try to get the edge databases with the provided IDs.
            var edgeDatabases = View.EdgeDatabases.Where(item => edgeDatabaseIds.Contains(item.Id));
            // Check if there weren't any edge databases found.
            if (!edgeDatabases.Any())
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "No edge databases could be found with the provided IDs.");
                // Redisplay the page.
                return Page();
            }
            // Get the provided seed node collection IDs.
            var seedNodeCollectionIds = Input.SeedNodeCollectionIds ?? Enumerable.Empty<string>();
            // Try to get the seed node collections with the provided IDs.
            var seedNodeCollections = View.SeedNodeCollections.Where(item => seedNodeCollectionIds.Contains(item.Id));
            // Define the data for generating the network.
            var data = string.Empty;
            // Check the database type of the network.
            if (View.IsGeneric)
            {
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
                    ModelState.AddModelError(string.Empty, "No edges could be found within the provided seed data.");
                    // Redisplay the page.
                    return Page();
                }
                // Serialize the seed data.
                data = JsonSerializer.Serialize(items
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
            }
            else
            {
                // Try to deserialize the seed data.
                if (!Input.SeedData.TryDeserializeJsonObject<IEnumerable<string>>(out var items) || items == null)
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided seed data could not be deserialized.");
                    // Redisplay the page.
                    return Page();
                }
                // Check if there weren't any items found.
                if (!items.Any() && !seedNodeCollections.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "No items could be found within the provided seed data or the selected seed node collections.");
                    // Redisplay the page.
                    return Page();
                }
                // Serialize the seed data.
                data = JsonSerializer.Serialize(items
                    .Select(item => new NetworkNodeInputModel
                    {
                        Node = new NodeInputModel
                        {
                            Id = item
                        },
                        Type = "Seed"
                    }));
            }
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
                        Algorithm = Input.Algorithm,
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
                            new List<NetworkUserInputModel>(),
                        NetworkNodeCollections = seedNodeCollections
                            .Select(item => item.Id)
                            .Select(item => new NetworkNodeCollectionInputModel
                            {
                                NodeCollection = new NodeCollectionInputModel
                                {
                                    Id = item
                                },
                                Type = "Seed"
                            })
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
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 network of type \"{databaseType.Name}\" defined successfully and scheduled for generation.";
            // Check if there wasn't any ID returned.
            if (ids != null && ids.Any())
            {
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Details/Index", new { id = ids.First() });
            }
            // Redirect to the index page.
            return RedirectToPage("/Content/Created/Networks/Index");
        }
    }
}
