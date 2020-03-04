using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetControl4BioMed.Data;
using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;

namespace NetControl4BioMed.Pages.Content.Created.Networks.Details
{
    [Authorize]
    public class VisualizeModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly LinkGenerator _linkGenerator;

        public VisualizeModel(UserManager<User> userManager, ApplicationDbContext context, LinkGenerator linkGenerator)
        {
            _userManager = userManager;
            _context = context;
            _linkGenerator = linkGenerator;
        }

        public ViewModel View { get; set; }

        public class ViewModel
        {
            public Network Network { get; set; }

            public bool IsGeneric { get; set; }

            public string CytoscapeJson { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
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
            if (string.IsNullOrEmpty(id))
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No ID has been provided.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Get the item with the provided ID.
            var item = _context.Networks
                .Where(item => item.NetworkUsers.Any(item1 => item1.User == user))
                .Where(item => item.Id == id)
                .Include(item => item.NetworkNodes)
                    .ThenInclude(item => item.Node)
                .Include(item => item.NetworkEdges)
                    .ThenInclude(item => item.Edge)
                        .ThenInclude(item => item.EdgeNodes)
                            .ThenInclude(item => item.Node)
                .Include(item => item.NetworkDatabases)
                    .ThenInclude(item => item.Database)
                        .ThenInclude(item => item.DatabaseType)
                .FirstOrDefault();
            // Check if there was no item found.
            if (item == null)
            {
                // Display a message.
                TempData["StatusMessage"] = "Error: No item has been found with the provided ID.";
                // Redirect to the index page.
                return RedirectToPage("/Content/Created/Networks/Index");
            }
            // Define the view.
            View = new ViewModel
            {
                Network = item,
                IsGeneric = item.NetworkDatabases
                    .Any(item => item.Database.DatabaseType.Name == "Generic")
            };
            // Get the default values.
            var interactionType = View.Network.NetworkDatabases.FirstOrDefault().Database.DatabaseType.Name.ToLower();
            var nodeClasses = new List<string> { "node" };
            var edgeClasses = new List<string> { "edge" };
            // Get the required data.
            var elements = new CytoscapeViewModel.CytoscapeData.CytoscapeElements
            {
                Nodes = View.Network.NetworkNodes
                    .Select(item => item.Node)
                    .Select(item => new CytoscapeViewModel.CytoscapeData.CytoscapeElements.CytoscapeNode
                    {
                        Data = new CytoscapeViewModel.CytoscapeData.CytoscapeElements.CytoscapeNode.CytoscapeNodeData
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Href = View.IsGeneric ? string.Empty : _linkGenerator.GetPathByPage(page: "/Content/Data/Nodes/Details", values: new { id = item.Id }),
                            Alias = item.DatabaseNodeFieldNodes
                                .Where(item1 => item1.DatabaseNodeField.IsSearchable)
                                .Select(item1 => item1.Value)
                        },
                        Classes = nodeClasses.Concat(item.NetworkNodes.Select(item => item.Type.ToString().ToLower()))
                    }),
                Edges = View.Network.NetworkEdges
                    .Select(item => item.Edge)
                    .Select(item => new CytoscapeViewModel.CytoscapeData.CytoscapeElements.CytoscapeEdge
                    {
                        Data = new CytoscapeViewModel.CytoscapeData.CytoscapeElements.CytoscapeEdge.CytoscapeEdgeData
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Source = item.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Source)?.Node.Id,
                            Target = item.EdgeNodes.FirstOrDefault(item1 => item1.Type == EdgeNodeType.Target)?.Node.Id,
                            Interaction = interactionType
                        },
                        Classes = edgeClasses
                    })
            };
            // Define the view.
            View.CytoscapeJson = JsonSerializer.Serialize(elements, new JsonSerializerOptions { IgnoreNullValues = true });
            // Return the page.
            return Page();
        }
    }
}
