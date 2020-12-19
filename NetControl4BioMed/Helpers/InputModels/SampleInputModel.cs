using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a sample.
    /// </summary>
    public class SampleInputModel
    {
        /// <summary>
        /// Represents the ID of the sample.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the sample.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the sample.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the network name of the sample.
        /// </summary>
        public string NetworkName { get; set; }

        /// <summary>
        /// Gets or sets the network description of the sample.
        /// </summary>
        public string NetworkDescription { get; set; }

        /// <summary>
        /// Gets or sets the network algorithm of the sample.
        /// </summary>
        public string NetworkAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the network node database data of the sample.
        /// </summary>
        public string NetworkNodeDatabaseData { get; set; }

        /// <summary>
        /// Gets or sets the network edge database data of the sample.
        /// </summary>
        public string NetworkEdgeDatabaseData { get; set; }

        /// <summary>
        /// Gets or sets the network seed data of the sample.
        /// </summary>
        public string NetworkSeedData { get; set; }

        /// <summary>
        /// Gets or sets the network seed node collection data of the sample.
        /// </summary>
        public string NetworkSeedNodeCollectionData { get; set; }

        /// <summary>
        /// Gets or sets the analysis name of the sample.
        /// </summary>
        public string AnalysisName { get; set; }

        /// <summary>
        /// Gets or sets the analysis description of the sample.
        /// </summary>
        public string AnalysisDescription { get; set; }

        /// <summary>
        /// Gets or sets the analysis algorithm of the sample.
        /// </summary>
        public string AnalysisAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the analysis network data of the sample.
        /// </summary>
        public string AnalysisNetworkData { get; set; }

        /// <summary>
        /// Gets or sets the analysis source data of the sample.
        /// </summary>
        public string AnalysisSourceData { get; set; }

        /// <summary>
        /// Gets or sets the analysis source node collection data of the sample.
        /// </summary>
        public string AnalysisSourceNodeCollectionData { get; set; }

        /// <summary>
        /// Gets or sets the analysis target data of the sample.
        /// </summary>
        public string AnalysisTargetData { get; set; }

        /// <summary>
        /// Gets or sets the analysis target node collection data of the sample.
        /// </summary>
        public string AnalysisTargetNodeCollectionData { get; set; }

        /// <summary>
        /// Represents the sample databases of the sample.
        /// </summary>
        public IEnumerable<SampleDatabaseInputModel> SampleDatabases { get; set; }
    }
}
