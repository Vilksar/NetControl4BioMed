using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on a protein.
    /// </summary>
    public interface IProteinDependent
    {
        /// <summary>
        /// Represents the protein ID of the model.
        /// </summary>
        string ProteinId { get; set; }

        /// <summary>
        /// Represents the protein of the model.
        /// </summary>
        Protein Protein { get; set; }
    }
}
