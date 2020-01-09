using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a node collection used by a network.
    /// </summary>
    public enum NetworkNodeCollectionType
    {
        /// <summary>
        /// Represents a collection with nodes used as seed nodes.
        /// </summary>
        [Display(Name = "Seed", Description = "The nodes in the collection are seed nodes in the network.")]
        Seed
    }
}
