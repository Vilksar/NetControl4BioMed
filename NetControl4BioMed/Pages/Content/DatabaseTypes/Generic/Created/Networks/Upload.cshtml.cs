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

namespace NetControl4BioMed.Pages.Content.DatabaseTypes.Generic.Created.Networks
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
            public string SeedEdgeData { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string ReCaptchaToken { get; set; }
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public bool IsUserAuthenticated { get; set; }

            public IEnumerable<SampleItemModel> SampleItems { get; set; }
        }

        public class EdgeItemModel
        {
            public string SourceNode { get; set; }

            public string TargetNode { get; set; }
        }

        public class SampleItemModel
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string networkId = null, string sampleId = null)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the view.
            View = new ViewModel
            {
                IsUserAuthenticated = user != null,
                SampleItems = _context.Samples
                    .Where(item => item.SampleDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Select(item => new SampleItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Description = item.Description
                    })
            };
            // Get the available databases.
            var databases = _context.Databases
                .Where(item => item.DatabaseType.Name == "Generic")
                .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user));
            // Check if there weren't any databases available.
            if (!databases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Networks/Index");
            }
            // Check if there was a network provided.
            if (!string.IsNullOrEmpty(networkId))
            {
                // Try to get the network with the provided ID.
                var networks = _context.Networks
                    .Where(item => item.NetworkDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Where(item => item.IsPublic || item.NetworkUsers.Any(item1 => item1.User == user))
                    .Where(item => item.Id == networkId);
                // Check if there was an ID provided, but there was no network found.
                if (networks == null || !networks.Any())
                {
                    // Display a message.
                    TempData["StatusMessage"] = "Error: No network could be found with the provided ID, or you don't have access to it.";
                    // Redirect to the index page.
                    return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Networks/Index");
                }
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
                    SeedEdgeData = JsonSerializer.Serialize(networks
                        .Select(item => item.NetworkEdges)
                        .SelectMany(item => item)
                        .Select(item => new EdgeItemModel
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
                // Try to get the sample with the provided ID.
                var sample = _context.Samples
                    .Where(item => item.SampleDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .FirstOrDefault(item => item.Id == sampleId);
                // Check if there was an ID provided, but there was no sample found.
                if (sample == null)
                {
                    // Display a message.
                    TempData["StatusMessage"] = "Error: No sample could be found with the provided ID.";
                    // Redirect to the index page.
                    return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Networks/Index");
                }
                // Define the input.
                Input = new InputModel
                {
                    Name = sample.NetworkName,
                    Description = sample.NetworkDescription,
                    IsPublic = !View.IsUserAuthenticated,
                    SeedEdgeData = sample.NetworkSeedEdgeData
                };
            }
            else
            {
                // Define the input.
                Input = new InputModel
                {
                    IsPublic = !View.IsUserAuthenticated,
                    SeedEdgeData = JsonSerializer.Serialize(Enumerable.Empty<EdgeItemModel>())
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
                SampleItems = _context.Samples
                    .Where(item => item.SampleDatabases.Any(item1 => item1.Database.DatabaseType.Name == "Generic"))
                    .Select(item => new SampleItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Description = item.Description
                    })
            };
            // Get the available databases.
            var databases = _context.Databases
                .Where(item => item.DatabaseType.Name == "Generic")
                .Where(item => item.IsPublic || item.DatabaseUsers.Any(item1 => item1.User == user));
            // Check if there weren't any databases available.
            if (!databases.Any())
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: A new network can't be created, as there are no databases available.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Networks/Index");
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
            // Try to deserialize the seed data.
            if (!Input.SeedEdgeData.TryDeserializeJsonObject<IEnumerable<EdgeItemModel>>(out var items) || items == null)
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
                        NetworkDatabases = databases
                            .Select(item => item.Id)
                            .Select(item => new NetworkDatabaseInputModel
                            {
                                Database = new DatabaseInputModel
                                {
                                    Id = item
                                },
                                Type = "Node"
                            })
                            .Concat(databases
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
                TempData["StatusMessage"] = $"Success: 1 generic network defined successfully with the ID \"{ids.First()}\" and scheduled for generation.";
                // Redirect to the index page.
                return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Networks/Details/Index", new { id = ids.First() });
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 generic network of type defined successfully and scheduled for generation.";
            // Redirect to the index page.
            return RedirectToPage("/Content/DatabaseTypes/Generic/Created/Networks/Index");
        }
    }
}
