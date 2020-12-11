using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on a node collection.
    /// </summary>
    public interface INodeCollectionDependent
    {
        /// <summary>
        /// Represents the node collection ID of the model.
        /// </summary>
        string NodeCollectionId { get; set; }

        /// <summary>
        /// Represents the network of the model.
        /// </summary>
        NodeCollection NodeCollection { get; set; }
    }
}
