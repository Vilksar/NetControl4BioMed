using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Analyses.Greedy
{
    /// <summary>
    /// Represents the model of the parameters used by the algorithm.
    /// </summary>
    public class Parameters : IValidatableObject
    {
        /// <summary>
        /// Gets or sets the random seed to be used throughout the algorithm.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "The value must be a positive integer.")]
        [Required(ErrorMessage = "This field is required.")]
        public int RandomSeed { get; set; } = new Random().Next();

        /// <summary>
        /// Gets or sets the maximum length of any path between a source node and a target node.
        /// </summary>
        [Range(0, 25, ErrorMessage = "The value must be between {1} and {2}.")]
        [Required(ErrorMessage = "This field is required.")]
        public int MaximumPathLength { get; set; } = 5;

        /// <summary>
        /// Gets or sets the number of times that each heuristic will be repeated in one iteration..
        /// </summary>
        [Range(1, 3, ErrorMessage = "The value must be between {1} and {2}.")]
        [Required(ErrorMessage = "This field is required.")]
        public int Repeats { get; set; } = 1;

        /// <summary>
        /// Gets or sets the search heuristics.
        /// </summary>
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "This field is required.")]
        public string Heuristics { get; set; } = JsonSerializer.Serialize(new List<List<string>>
        {
            new List<string> { "A" },
            new List<string> { "B" },
            new List<string> { "C" },
            new List<string> { "D" },
            new List<string> { "Z" }
        });

        /// <summary>
        /// Checks if the parameters are valid.
        /// </summary>
        /// <param name="validationContext">Represents the validation context.</param>
        /// <returns>Returns a list with the validation errors.</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Check if the string is not a valid JSON array.
            if (!Heuristics.TryDeserializeJsonObject<IEnumerable<IEnumerable<string>>>(out var heuristics))
            {
                // Return an error.
                yield return new ValidationResult("The value is not a valid JSON string.", new List<string> { nameof(Heuristics) });
            }
            // Check if the heuristics are not valid.
            if (heuristics == null || !heuristics.Any() || !heuristics.SelectMany(item => item).Distinct().All(item => PossibleHeuristics.ContainsKey(item)))
            {
                // Return an error.
                yield return new ValidationResult("The value contains invalid characters.", new List<string> { nameof(Heuristics) });
            }
        }

        /// <summary>
        /// Gets the possible heuristics.
        /// </summary>
        [JsonIgnore]
        public static Dictionary<string, string> PossibleHeuristics { get; } = new Dictionary<string, string>()
        {
            { "A", "Edges from previously seen source nodes" },
            { "B", "Edges from any source node" },
            { "C", "Edges from any controlling node" },
            { "D", "Edges from previously seen nodes" },
            { "E", "Edges from a node that has not appeared in the current path" },
            { "Z", "Any possible edge" }
        };
    }
}
