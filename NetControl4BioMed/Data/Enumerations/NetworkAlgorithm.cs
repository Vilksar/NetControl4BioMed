using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible algorithms used to generate a network.
    /// </summary>
    public enum NetworkAlgorithm
    {
        /// <summary>
        /// Represents a network which hasn't been generated, but provided by the user.
        /// </summary>
        [Display(Name = "None", Description = "The network has been provided by the user.")]
        None,

        /// <summary>
        /// Represents a network which contains all edges involving at least one seed node.
        /// </summary>
        [Display(Name = "Neighbors", Description = "The network contains all edges involving at least one seed node.")]
        Neighbors,

        /// <summary>
        /// Represents a network which contains only the edges between the seed nodes.
        /// </summary>
        [Display(Name = "Gap 0", Description = "The network contains only the edges between the seed nodes.")]
        Gap0,

        /// <summary>
        /// Represents a network which contains the edges between the seed nodes, with at most one additional node in-between.
        /// </summary>
        [Display(Name = "Gap 1", Description = "The network contains the edges between the seed nodes, with at most one additional node in-between.")]
        Gap1,

        /// <summary>
        /// Represents a network which contains the edges between the seed nodes, with at most two additional nodes in-between.
        /// </summary>
        [Display(Name = "Gap 2", Description = "The network contains the edges between the seed nodes, with at most two additional nodes in-between.")]
        Gap2,

        /// <summary>
        /// Represents a network which contains the edges between the seed nodes, with at most three additional nodes in-between.
        /// </summary>
        [Display(Name = "Gap 3", Description = "The network contains the edges between the seed nodes, with at most three additional nodes in-between.")]
        Gap3,

        /// <summary>
        /// Represents a network which contains the edges between the seed nodes, with at most four additional nodes in-between.
        /// </summary>
        [Display(Name = "Gap 4", Description = "The network contains the edges between the seed nodes, with at most four additional nodes in-between.")]
        Gap4
    }
}
