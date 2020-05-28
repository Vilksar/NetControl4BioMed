using System.Collections.Generic;

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
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the analysis.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the analysis.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the algorithm of the analysis.
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// Represents the data of the analysis.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Represents the maximum number of iterations of the analysis.
        /// </summary>
        public int MaximumIterations { get; set; }

        /// <summary>
        /// Represents the maximum number of iterations without improvement of the analysis.
        /// </summary>
        public int MaximumIterationsWithoutImprovement { get; set; }

        /// <summary>
        /// Represents the parameters of the analysis.
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// Represents the analysis users of the analysis.
        /// </summary>
        public IEnumerable<AnalysisUserInputModel> AnalysisUsers { get; set; }

        /// <summary>
        /// Represents the analysis user invitations of the analysis.
        /// </summary>
        public IEnumerable<AnalysisUserInvitationInputModel> AnalysisUserInvitations { get; set; }

        /// <summary>
        /// Represents the analysis databases of the analysis.
        /// </summary>
        public IEnumerable<AnalysisDatabaseInputModel> AnalysisDatabases { get; set; }

        /// <summary>
        /// Represents the analysis nodes of the analysis.
        /// </summary>
        public IEnumerable<AnalysisNodeInputModel> AnalysisNodes { get; set; }

        /// <summary>
        /// Represents the analysis edges of the analysis.
        /// </summary>
        public IEnumerable<AnalysisEdgeInputModel> AnalysisEdges { get; set; }

        /// <summary>
        /// Represents the analysis node collections of the analysis.
        /// </summary>
        public IEnumerable<AnalysisNodeCollectionInputModel> AnalysisNodeCollections { get; set; }

        /// <summary>
        /// Represents the analysis networks of the analysis.
        /// </summary>
        public IEnumerable<AnalysisNetworkInputModel> AnalysisNetworks { get; set; }
    }
}
