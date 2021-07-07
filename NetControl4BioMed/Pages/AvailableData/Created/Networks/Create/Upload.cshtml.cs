using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
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

namespace NetControl4BioMed.Pages.AvailableData.Created.Networks.Create
{
    [RequestFormLimits(MultipartBodyLengthLimit = 16 * 1024 * 1024)]
    public class UploadModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<User> _userManager;
        private readonly IReCaptchaChecker _reCaptchaChecker;

        public UploadModel(IServiceProvider serviceProvider, UserManager<User> userManager, IReCaptchaChecker reCaptchaChecker)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _reCaptchaChecker = reCaptchaChecker;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel : IValidatableObject
        {
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public bool IsPublic { get; set; }

            [DataType(DataType.Upload)]
            [Required(ErrorMessage = "This field is required.")]
            public IFormFile FormFile { get; set; }

            [DataType(DataType.Text)]
            [Required(ErrorMessage = "This field is required.")]
            public string ReCaptchaToken { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                // Check if there is no file selected.
                if (FormFile == null)
                {
                    // Return an error.
                    yield return new ValidationResult("The provided file could not be found.", new List<string> { string.Empty });
                }
                // Check if the file does not have an allowed extension.
                if (!new List<string> { ".cyjs", ".cx" }.Contains(System.IO.Path.GetExtension(FormFile.FileName).ToLower()))
                {
                    // Return an error.
                    yield return new ValidationResult($"The provided file does not have a valid extension.", new List<string> { string.Empty });
                }
                // Check if the file is larger than the maximum size.
                if (16 * 1024 * 1024 < FormFile.Length)
                {
                    // Return an error.
                    yield return new ValidationResult("The provided file is too large.", new List<string> { string.Empty });
                }
            }
        }

        public class ItemModel
        {
            public string SourceNode { get; set; }

            public string TargetNode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
            // Define the input.
            Input = new InputModel
            {
                IsPublic = user == null
            };
            // Return the page.
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);
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
                ModelState.AddModelError(string.Empty, "You are not logged in, so the network must be set as public.");
                // Redisplay the page.
                return Page();
            }
            // Get the extension of the file and its memory stream.
            var extension = System.IO.Path.GetExtension(Input.FormFile.FileName).ToLower();
            // Define the task to create.
            var task = new NetworksTask
            {
                Scheme = HttpContext.Request.Scheme,
                HostValue = HttpContext.Request.Host.Value
            };
            // Check the type of the file that was provided.
            if (extension == ".cyjs")
            {
                // Define the new object.
                var viewModel = new FileCyjsViewModel();
                // Try to deserialize the data.
                try
                {
                    // Get the deserialized object and assign the output value.
                    viewModel = await JsonSerializer.DeserializeAsync<FileCyjsViewModel>(Input.FormFile.OpenReadStream());
                }
                catch (Exception exception)
                {
                    // Define the messages to return.
                    var messages = new List<string> { "An error occurred while reading the file." };
                    // Build the exception message.
                    while (exception != null)
                    {
                        // Update the messages and the exception.
                        messages.Add(exception.Message);
                        exception = exception.InnerException;
                    }
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, string.Join(" ", messages.Where(item => !string.IsNullOrEmpty(item))));
                    // Redisplay the page.
                    return Page();
                }
                // Get the network details.
                var networkDetails = viewModel.Data;
                var networkName = networkDetails?.Name;
                var networkDescription = networkDetails?.Description;
                // Get the proteins from the data.
                var proteins = viewModel.Elements?.Nodes?
                    .Where(item => item.Data != null && !string.IsNullOrEmpty(item.Data.Id) && !string.IsNullOrEmpty(item.Data.Name))
                    .ToDictionary(item => item.Data.Id, item => item.Data.Name);
                // Check if there were no proteins found within the data.
                if (proteins == null || !proteins.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided file does not contain any valid proteins.");
                    // Redisplay the page.
                    return Page();
                }
                // Get the interactions from the data.
                var interactions = viewModel.Elements?.Edges?
                    .Where(item => item.Data != null && !string.IsNullOrEmpty(item.Data.Source) && !string.IsNullOrEmpty(item.Data.Target))
                    .Select(item => new ItemModel
                    {
                        SourceNode = proteins.GetValueOrDefault(item.Data.Source),
                        TargetNode = proteins.GetValueOrDefault(item.Data.Target)
                    })
                    .Where(item => !string.IsNullOrEmpty(item.SourceNode) && !string.IsNullOrEmpty(item.TargetNode));
                // Check if there were no interactions found within the data.
                if (interactions == null || !interactions.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided file does not contain any valid interactions.");
                    // Redisplay the page.
                    return Page();
                }
                // Serialize the seed data.
                var data = JsonSerializer.Serialize(interactions
                    .Select(item => new NetworkInteractionInputModel
                    {
                        Interaction = new InteractionInputModel
                        {
                            InteractionProteins = new List<InteractionProteinInputModel>
                            {
                                new InteractionProteinInputModel
                                {
                                    Protein = new ProteinInputModel
                                    {
                                        Id = item.SourceNode
                                    },
                                    Type = "Source"
                                },
                                new InteractionProteinInputModel
                                {
                                    Protein = new ProteinInputModel
                                    {
                                        Id = item.TargetNode
                                    },
                                    Type = "Target"
                                }
                            }
                        }
                    }));
                // Define a new task.
                task.Items = new List<NetworkInputModel>
                {
                    new NetworkInputModel
                    {
                        Name = !string.IsNullOrEmpty(networkName) ? networkName : $"Network from CytoscapeJS on {DateTime.Now:yyyy-MM-dd} at {DateTime.Now:HH-mm-ss}",
                        Description = !string.IsNullOrEmpty(networkDescription) ? networkDescription : $"This network was uploaded from a CytoscapeJS file on {DateTime.Now:D} at {DateTime.Now:T}.",
                        IsPublic = Input.IsPublic,
                        Algorithm = NetworkAlgorithm.None.ToString(),
                        Data = data,
                        NetworkUsers = user != null ?
                            new List<NetworkUserInputModel>
                            {
                                new NetworkUserInputModel
                                {
                                    User = new UserInputModel
                                    {
                                        Id = user.Id
                                    },
                                    Email = user.Email
                                }
                            } :
                            new List<NetworkUserInputModel>()
                    }
                };
            }
            // Check the type of the file that was provided.
            else if (extension == ".cx")
            {
                // Define the new object.
                var viewModel = new List<FileCxViewModel.CxBaseObject>();
                // Try to deserialize the data.
                try
                {
                    // Get the deserialized object and assign the output value.
                    viewModel = await JsonSerializer.DeserializeAsync<List<FileCxViewModel.CxBaseObject>>(Input.FormFile.OpenReadStream());
                }
                catch (Exception exception)
                {
                    // Define the messages to return.
                    var messages = new List<string> { "An error occurred while reading the file." };
                    // Build the exception message.
                    while (exception != null)
                    {
                        // Update the messages and the exception.
                        messages.Add(exception.Message);
                        exception = exception.InnerException;
                    }
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, string.Join(" ", messages.Where(item => !string.IsNullOrEmpty(item))));
                    // Redisplay the page.
                    return Page();
                }
                // Get the network details.
                var networkDetails = viewModel.FirstOrDefault(item => item.NetworkAttributes != null)?.NetworkAttributes;
                var networkName = networkDetails?.FirstOrDefault(item => item != null && item.Name == "name")?.Value;
                var networkDescription = networkDetails?.FirstOrDefault(item => item != null && item.Name == "description")?.Value;
                // Get the proteins from the data.
                var proteins = viewModel.FirstOrDefault(item => item.Nodes != null)?.Nodes
                    .Where(item => item != null && !string.IsNullOrEmpty(item.Name))
                    .ToDictionary(item => item.Id, item => item.Name);
                // Check if there were no proteins found within the data.
                if (proteins == null || !proteins.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided file does not contain any valid proteins.");
                    // Redisplay the page.
                    return Page();
                }
                // Get the interactions from the data.
                var interactions = viewModel.FirstOrDefault(item => item.Edges != null)?.Edges
                    .Where(item => item != null)
                    .Select(item => new ItemModel
                    {
                        SourceNode = proteins.GetValueOrDefault(item.Source),
                        TargetNode = proteins.GetValueOrDefault(item.Target)
                    })
                    .Where(item => !string.IsNullOrEmpty(item.SourceNode) && !string.IsNullOrEmpty(item.TargetNode));
                // Check if there were no interactions found within the data.
                if (interactions == null || !interactions.Any())
                {
                    // Add an error to the model.
                    ModelState.AddModelError(string.Empty, "The provided file does not contain any valid interactions.");
                    // Redisplay the page.
                    return Page();
                }
                // Serialize the seed data.
                var data = JsonSerializer.Serialize(interactions
                    .Select(item => new NetworkInteractionInputModel
                    {
                        Interaction = new InteractionInputModel
                        {
                            InteractionProteins = new List<InteractionProteinInputModel>
                            {
                                new InteractionProteinInputModel
                                {
                                    Protein = new ProteinInputModel
                                    {
                                        Id = item.SourceNode
                                    },
                                    Type = "Source"
                                },
                                new InteractionProteinInputModel
                                {
                                    Protein = new ProteinInputModel
                                    {
                                        Id = item.TargetNode
                                    },
                                    Type = "Target"
                                }
                            }
                        }
                    }));
                // Define a new task.
                task.Items = new List<NetworkInputModel>
                {
                    new NetworkInputModel
                    {
                        Name = !string.IsNullOrEmpty(networkName) ? networkName : $"Network from CX on {DateTime.Now:yyyy-MM-dd} at {DateTime.Now:HH-mm-ss}",
                        Description = !string.IsNullOrEmpty(networkDescription) ? networkDescription : $"This network was uploaded from a CX file on {DateTime.Now:D} at {DateTime.Now:T}.",
                        IsPublic = Input.IsPublic,
                        Algorithm = NetworkAlgorithm.None.ToString(),
                        Data = data,
                        NetworkUsers = user != null ?
                            new List<NetworkUserInputModel>
                            {
                                new NetworkUserInputModel
                                {
                                    User = new UserInputModel
                                    {
                                        Id = user.Id
                                    },
                                    Email = user.Email
                                }
                            } :
                            new List<NetworkUserInputModel>()
                    }
                };
            }
            // Check if the type is not valid.
            else
            {
                // Add an error to the model.
                ModelState.AddModelError(string.Empty, "The provided file extension is not valid.");
                // Redisplay the page.
                return Page();
            }
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
                TempData["StatusMessage"] = $"Success: 1 network uploaded successfully and scheduled for generation.";
                // Redirect to the index page.
                return RedirectToPage("/AvailableData/Created/Networks/Index");
            }
            // Display a message.
            TempData["StatusMessage"] = $"Success: 1 network uploaded successfully with the ID \"{ids.First()}\" and scheduled for generation.";
            // Redirect to the index page.
            return RedirectToPage("/AvailableData/Created/Networks/Details/Index", new { id = ids.First() });
        }
    }
}
