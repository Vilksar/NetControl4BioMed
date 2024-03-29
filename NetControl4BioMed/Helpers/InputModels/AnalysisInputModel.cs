﻿using System.Collections.Generic;

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
        /// Represents the public availability of the analysis.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the demonstration availability of the analysis.
        /// </summary>
        public bool IsDemonstration { get; set; }

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
        /// Gets or sets the network used by the analysis.
        /// </summary>
        public NetworkInputModel Network { get; set; }

        /// <summary>
        /// Represents the analysis users of the analysis.
        /// </summary>
        public IEnumerable<AnalysisUserInputModel> AnalysisUsers { get; set; }

        /// <summary>
        /// Represents the analysis databases of the analysis.
        /// </summary>
        public IEnumerable<AnalysisDatabaseInputModel> AnalysisDatabases { get; set; }

        /// <summary>
        /// Represents the analysis proteins of the analysis.
        /// </summary>
        public IEnumerable<AnalysisProteinInputModel> AnalysisProteins { get; set; }

        /// <summary>
        /// Represents the analysis interactions of the analysis.
        /// </summary>
        public IEnumerable<AnalysisInteractionInputModel> AnalysisInteractions { get; set; }

        /// <summary>
        /// Represents the analysis protein collections of the analysis.
        /// </summary>
        public IEnumerable<AnalysisProteinCollectionInputModel> AnalysisProteinCollections { get; set; }

        /// <summary>
        /// Represents the control paths of the analysis.
        /// </summary>
        public IEnumerable<ControlPathInputModel> ControlPaths { get; set; }
    }
}
