using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a control path.
    /// </summary>
    public class ControlPathInputModel
    {
        /// <summary>
        /// Represents the ID of the control path.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the analysis of the control path.
        /// </summary>
        public AnalysisInputModel Analysis { get; set; }

        /// <summary>
        /// Represents the paths of the control path.
        /// </summary>
        public IEnumerable<PathInputModel> Paths { get; set; }
    }
}
