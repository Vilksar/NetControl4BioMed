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
        /// Represents a network which contains all interactions involving at least one seed protein.
        /// </summary>
        [Display(Name = "Neighbors", Description = "The network contains all interactions involving at least one seed protein.")]
        Neighbors,

        /// <summary>
        /// Represents a network which contains only the interactions between the seed proteins.
        /// </summary>
        [Display(Name = "Gap 0", Description = "The network contains only the interactions between the seed proteins.")]
        Gap0,

        /// <summary>
        /// Represents a network which contains the interactions between the seed proteins, with at most one additional protein in-between.
        /// </summary>
        [Display(Name = "Gap 1", Description = "The network contains the interactions between the seed proteins, with at most one additional protein in-between.")]
        Gap1,

        /// <summary>
        /// Represents a network which contains the interactions between the seed proteins, with at most two additional proteins in-between.
        /// </summary>
        [Display(Name = "Gap 2", Description = "The network contains the interactions between the seed proteins, with at most two additional proteins in-between.")]
        Gap2,

        /// <summary>
        /// Represents a network which contains the interactions between the seed proteins, with at most three additional proteins in-between.
        /// </summary>
        [Display(Name = "Gap 3", Description = "The network contains the interactions between the seed proteins, with at most three additional proteins in-between.")]
        Gap3,

        /// <summary>
        /// Represents a network which contains the interactions between the seed proteins, with at most four additional proteins in-between.
        /// </summary>
        [Display(Name = "Gap 4", Description = "The network contains the interactions between the seed proteins, with at most four additional proteins in-between.")]
        Gap4
    }
}
