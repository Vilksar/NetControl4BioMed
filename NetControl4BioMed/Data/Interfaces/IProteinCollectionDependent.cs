using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on a protein collection.
    /// </summary>
    public interface IProteinCollectionDependent
    {
        /// <summary>
        /// Represents the protein collection ID of the model.
        /// </summary>
        string ProteinCollectionId { get; set; }

        /// <summary>
        /// Represents the protein collection of the model.
        /// </summary>
        ProteinCollection ProteinCollection { get; set; }
    }
}
