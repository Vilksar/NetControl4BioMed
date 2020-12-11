using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on a node.
    /// </summary>
    public interface INodeDependent
    {
        /// <summary>
        /// Represents the node ID of the model.
        /// </summary>
        string NodeId { get; set; }

        /// <summary>
        /// Represents the node of the model.
        /// </summary>
        Node Node { get; set; }
    }
}
