using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of an analysis.
    /// </summary>
    public class Analysis : INetworkDependent
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the analysis.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the analysis has been created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the date when the analysis has started.
        /// </summary>
        public DateTime? DateTimeStarted { get; set; }

        /// <summary>
        /// Gets or sets the date when the analysis has ended.
        /// </summary>
        public DateTime? DateTimeEnded { get; set; }

        /// <summary>
        /// Gets or sets the date when the network will be automatically deleted.
        /// </summary>
        public DateTime DateTimeToDelete { get; set; }

        /// <summary>
        /// Gets or sets the name of the analysis.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the analysis.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the public availability of the analysis.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the demonstration availability of the analysis.
        /// </summary>
        public bool IsDemonstration { get; set; }

        /// <summary>
        /// Gets or sets the current status of the analysis.
        /// </summary>
        public AnalysisStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the message log of the analysis.
        /// </summary>
        public string Log { get; set; }

        /// <summary>
        /// Gets or sets the data used to generate the analysis.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the current iteration of the algorithm that the analysis uses.
        /// </summary>
        public int CurrentIteration { get; set; }

        /// <summary>
        /// Gets or sets the current iteration without improvement of the algorithm that the analysis uses.
        /// </summary>
        public int CurrentIterationWithoutImprovement { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of iterations.
        /// </summary>
        public int MaximumIterations { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of iterations without improvement.
        /// </summary>
        public int MaximumIterationsWithoutImprovement { get; set; }

        /// <summary>
        /// Gets or sets the algorithm used by the analysis.
        /// </summary>
        public AnalysisAlgorithm Algorithm { get; set; }

        /// <summary>
        /// Gets or sets the parameters of the algorithm used by the analysis, with an underlying format of Parameter.
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// Gets or sets the owner ID of the analysis.
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the owner of the analysis.
        /// </summary>
        public User Owner { get; set; }

        /// <summary>
        /// Gets or sets the network ID used by the analysis.
        /// </summary>
        public string NetworkId { get; set; }

        /// <summary>
        /// Gets or sets the network used by the analysis.
        /// </summary>
        public Network Network { get; set; }

        /// <summary>
        /// Gets or sets the registered users that have access to the analysis.
        /// </summary>
        public ICollection<AnalysisUser> AnalysisUsers { get; set; }

        /// <summary>
        /// Gets or sets the databases which are used by the analysis.
        /// </summary>
        public ICollection<AnalysisDatabase> AnalysisDatabases { get; set; }

        /// <summary>
        /// Gets or sets the proteins which appear in the network corresponding to the analysis.
        /// </summary>
        public ICollection<AnalysisProtein> AnalysisProteins { get; set; }

        /// <summary>
        /// Gets or sets the interactions which appear in the network corresponding to the analysis.
        /// </summary>
        public ICollection<AnalysisInteraction> AnalysisInteractions { get; set; }

        /// <summary>
        /// Gets or sets the protein collections which are used by the analysis.
        /// </summary>
        public ICollection<AnalysisProteinCollection> AnalysisProteinCollections { get; set; }

        /// <summary>
        /// Gets or sets the control paths found by the analysis.
        /// </summary>
        public ICollection<ControlPath> ControlPaths { get; set; }
    }
}
