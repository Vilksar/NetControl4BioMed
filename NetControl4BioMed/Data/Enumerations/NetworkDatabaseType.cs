using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a database used by a network.
    /// </summary>
    public enum NetworkDatabaseType
    {
        /// <summary>
        /// Represents a database whose nodes are used by the network.
        /// </summary>
        [Display(Name = "Node", Description = "The database's nodes are used by the network.")]
        Node,

        /// <summary>
        /// Represents a database whose edges are used by the network.
        /// </summary>
        [Display(Name = "Edge", Description = "The database's edges are used by the network.")]
        Edge
    }
}
