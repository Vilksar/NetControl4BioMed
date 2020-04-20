using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model of a data deleter.
    /// </summary>
    public class DataDeleterViewModel
    {
        /// <summary>
        /// Gets or sets the node deletion status.
        /// </summary>
        public bool DeleteNodes { get; set; }

        /// <summary>
        /// Gets or sets the node deletion status.
        /// </summary>
        public bool DeleteEdges { get; set; }

        /// <summary>
        /// Gets or sets the node deletion status.
        /// </summary>
        public bool DeleteNodeCollections { get; set; }

        /// <summary>
        /// Gets or sets the node deletion status.
        /// </summary>
        public bool DeleteNetworks { get; set; }

        /// <summary>
        /// Gets or sets the node deletion status.
        /// </summary>
        public bool DeleteAnalyses { get; set; }
    }
}
