using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a path.
    /// </summary>
    public class PathInputModel
    {
        /// <summary>
        /// Represents the ID of the path.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the control path of the path.
        /// </summary>
        public ControlPathInputModel ControlPath { get; set; }

        /// <summary>
        /// Represents the proteins of the path.
        /// </summary>
        public IEnumerable<PathProteinInputModel> PathProteins { get; set; }

        /// <summary>
        /// Represents the interactions of the path.
        /// </summary>
        public IEnumerable<PathInteractionInputModel> PathInteractions { get; set; }
    }
}
