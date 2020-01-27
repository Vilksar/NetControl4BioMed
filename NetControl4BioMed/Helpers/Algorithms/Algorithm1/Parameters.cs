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
        /// Gets or sets the random seed.
        /// </summary>
        public int RandomSeed { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of iterations.
        /// </summary>
        public int MaximumIterations { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of iterations without improvement.
        /// </summary>
        public int MaximumIterationsWithoutImprovement { get; set; }

        /// <summary>
        /// Gets or sets the maximum path length.
        /// </summary>
        public int MaximumPathLength { get; set; }

        /// <summary>
        /// Gets or sets the number of repeats.
        /// </summary>
        public int Repeats { get; set; }

        /// <summary>
        /// Gets or sets the heuristics.
        /// </summary>
        public List<List<string>> Heuristics { get; set; }

        /// <summary>
        /// Initializes a new default instance of the class.
        /// </summary>
        public Parameters()
        {
            // Define a new parameters model.
            var model = new ViewModel();
            // Assign the default value for each property.
            RandomSeed = model.RandomSeed.Value;
            MaximumIterations = model.MaximumIterations.Value;
            MaximumIterationsWithoutImprovement = model.MaximumIterationsWithoutImprovement.Value;
            MaximumPathLength = model.MaximumPathLength.Value;
            Repeats = model.Repeats.Value;
            Heuristics = JsonSerializer.Deserialize<List<List<string>>>(model.Heuristics);
        }

        public class ViewModel : IValidatableObject
        {
            /// <summary>
            /// Gets or sets the random seed.
            /// </summary>
            [Range(0, int.MaxValue, ErrorMessage = "The value must be a positive integer.")]
            public int? RandomSeed { get; set; } = new Random().Next();

            /// <summary>
            /// Gets or sets the maximum number of iterations.
            /// </summary>
            [Range(1, 20000, ErrorMessage = "The value must be between {1} and {2}.")]
            public int? MaximumIterations { get; set; } = 10000;

            /// <summary>
            /// Gets or sets the maximum number of iterations without improvement.
            /// </summary>
            [Range(1, 2000, ErrorMessage = "The value must be between {1} and {2}.")]
            public int? MaximumIterationsWithoutImprovement { get; set; } = 1000;

            /// <summary>
            /// Gets or sets the maximum path length.
            /// </summary>
            [Range(0, 25, ErrorMessage = "The value must be between {1} and {2}.")]
            public int? MaximumPathLength { get; set; } = 0;

            /// <summary>
            /// Gets or sets the number of repeats.
            /// </summary>
            [Range(1, 3, ErrorMessage = "The value must be between {1} and {2}.")]
            public int? Repeats { get; set; } = 1;

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
