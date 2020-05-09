using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an analysis.
    /// </summary>
    public class AnalysisInputModel
    {
        /// <summary>
        /// Represents the ID of the analysis.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Id { get; set; }

        /// <summary>
        /// Represents the database type ID of the analysis.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string DatabaseTypeId { get; set; }

        /// <summary>
        /// Represents the name of the analysis.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the analysis.
        /// </summary>
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        /// <summary>
        /// Represents the algorithm of the analysis.
        /// </summary>
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "This field is required.")]
        public string Algorithm { get; set; }

        /// <summary>
        /// Represents the source data of the analysis.
        /// </summary>
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "This field is required.")]
        public string SourceData { get; set; }

        /// <summary>
        /// Represents the target data of the analysis.
        /// </summary>
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "This field is required.")]
        public string TargetData { get; set; }

        /// <summary>
        /// Represents the maximum number of iterations of the analysis.
        /// </summary>
        [Range(0, 10000, ErrorMessage = "The value must be a positive integer lower than 10000.")]
        [Required(ErrorMessage = "This field is required.")]
        public int MaximumIterations { get; set; }

        /// <summary>
        /// Represents the maximum number of iterations without improvement of the analysis.
        /// </summary>
        [Range(0, 1000, ErrorMessage = "The value must be a positive integer lower than 1000.")]
        [Required(ErrorMessage = "This field is required.")]
        public int MaximumIterationsWithoutImprovement { get; set; }

        /// <summary>
        /// Represents the parameters of the analysis.
        /// </summary>
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "This field is required.")]
        public string Parameters { get; set; }

        /// <summary>
        /// Represents the analysis networks of the analysis.
        /// </summary>
        public IEnumerable<AnalysisNetworkInputModel> AnalysisNetworks { get; set; }

        /// <summary>
        /// Represents the analysis node collections of the analysis.
        /// </summary>
        public IEnumerable<AnalysisNodeCollectionInputModel> AnalysisNodeCollections { get; set; }
    }
}
