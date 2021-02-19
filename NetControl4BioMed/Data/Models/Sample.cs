using NetControl4BioMed.Data.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a sample.
    /// </summary>
    public class Sample
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the sample.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the sample was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the sample.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the sample.
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
        public NetworkAlgorithm NetworkAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the network node database data of the sample.
        /// </summary>
        public string NetworkNodeDatabaseData { get; set; }

        /// <summary>
        /// Gets or sets the network edge database data of the sample.
        /// </summary>
        public string NetworkEdgeDatabaseData { get; set; }

        /// <summary>
        /// Gets or sets the network seed node data of the sample.
        /// </summary>
        public string NetworkSeedNodeData { get; set; }

        /// <summary>
        /// Gets or sets the network seed edge data of the sample.
        /// </summary>
        public string NetworkSeedEdgeData { get; set; }

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
        public AnalysisAlgorithm AnalysisAlgorithm { get; set; }

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
        /// Gets or sets the databases that the sample uses.
        /// </summary>
        public ICollection<SampleDatabase> SampleDatabases { get; set; }
    }
}
