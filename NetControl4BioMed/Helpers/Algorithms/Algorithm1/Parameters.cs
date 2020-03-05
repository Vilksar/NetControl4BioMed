using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Algorithm1
{
    /// <summary>
    /// Represents the model of the parameters used by the algorithm.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// Gets or sets the random seed to be used throughout the algorithm.
        /// </summary>
        [Display(Name = "Random seed", Description = "The random seed to be used throughout the algorithm.")]
        public int RandomSeed { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of any path between a source node and a target node.
        /// </summary>
        [Display(Name = "Maximum path length", Description = "The maximum length of any path between a source node and a target node.")]
        public int MaximumPathLength { get; set; }

        /// <summary>
        /// Gets or sets the number of times that each heuristic will be repeated in one iteration..
        /// </summary>
        [Display(Name = "Repeats", Description = "The number of times that each heuristic will be repeated in one iteration.")]
        public int Repeats { get; set; }

        /// <summary>
        /// Gets or sets the search heuristics.
        /// </summary>
        [Display(Name = "Heuristics", Description = "The search heuristics.")]
        public List<List<string>> Heuristics { get; set; }

        /// <summary>
        /// Initializes a new default instance of the class.
        /// </summary>
        public Parameters()
        {
            // Define a new parameters model.
            var model = new ViewModel();
            // Assign the default value for each property.
            RandomSeed = model.RandomSeed;
            MaximumPathLength = model.MaximumPathLength;
            Repeats = model.Repeats;
            Heuristics = JsonSerializer.Deserialize<List<List<string>>>(model.Heuristics);
        }

        public class ViewModel : IValidatableObject
        {
            /// <summary>
            /// Gets or sets the random seed.
            /// </summary>
            [Range(0, int.MaxValue, ErrorMessage = "The value must be a positive integer.")]
            public int RandomSeed { get; set; } = new Random().Next();

            /// <summary>
            /// Gets or sets the maximum path length.
            /// </summary>
            [Range(0, 25, ErrorMessage = "The value must be between {1} and {2}.")]
            public int MaximumPathLength { get; set; } = 0;

            /// <summary>
            /// Gets or sets the number of repeats.
            /// </summary>
            [Range(1, 3, ErrorMessage = "The value must be between {1} and {2}.")]
            public int Repeats { get; set; } = 1;

            /// <summary>
            /// Gets or sets the heuristics.
            /// </summary>
            public string Heuristics { get; set; } = JsonSerializer.Serialize(new List<List<string>>
            {
                new List<string> { "A" },
                new List<string> { "B" },
                new List<string> { "C" },
                new List<string> { "D" },
                new List<string> { "E" },
                new List<string> { "F" },
                new List<string> { "G" },
                new List<string> { "Z" }
            });

            /// <summary>
            /// Gets the possible heuristics.
            /// </summary>
            public static Dictionary<string, string> PossibleHeuristics { get; } = new Dictionary<string, string>()
            {
                { "A", "Previously seen edges coming from drug-target nodes" },
                { "B", "Edges to any drug target node" },
                { "C", "Previously seen edges from controlling nodes" },
                { "D", "Edges to any controlling node" },
                { "E", "Previously seen edges" },
                { "F", "Edges to previously seen nodes" },
                { "G", "Edges to a node that has not appeared in the current path" },
                { "Z", "Any possible edge" }
            };

            /// <summary>
            /// Checks if the parameters are valid.
            /// </summary>
            /// <param name="validationContext">Represents the validation context.</param>
            /// <returns>Returns a list with the validation errors.</returns>
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                // Check if the string is a valid JSON array.
                if (Heuristics.TryDeserializeJsonObject<IEnumerable<IEnumerable<string>>>(out var _))
                {
                    // Return an error.
                    yield return new ValidationResult("The value is not a valid JSON string.", new List<string> { nameof(Heuristics) });
                }
            }
        }
    }
}
