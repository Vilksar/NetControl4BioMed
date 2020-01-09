using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of an edge.
    /// </summary>
    public class Edge
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the edge.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the edge has been created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the edge.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the edge.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the nodes which appear in the edge.
        /// </summary>
        public ICollection<EdgeNode> EdgeNodes { get; set; }

        /// <summary>
        /// Gets or sets the networks which contain the edge.
        /// </summary>
        public ICollection<NetworkEdge> NetworkEdges { get; set; }

        /// <summary>
        /// Gets or sets the analyses which contain the edge.
        /// </summary>
        public ICollection<AnalysisEdge> AnalysisEdges { get; set; }

        /// <summary>
        /// Gets or sets the paths which contain the edge.
        /// </summary>
        public ICollection<PathEdge> PathEdges { get; set; }

        /// <summary>
        /// Gets or sets the edge database fields which have a value corresponding to the edge.
        /// </summary>
        public ICollection<DatabaseEdgeFieldEdge> DatabaseEdgeFieldEdges { get; set; }
    }
}
