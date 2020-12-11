using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on an edge.
    /// </summary>
    public interface IEdgeDependent
    {
        /// <summary>
        /// Represents the edge ID of the model.
        /// </summary>
        string EdgeId { get; set; }

        /// <summary>
        /// Represents the edge of the model.
        /// </summary>
        Edge Edge { get; set; }
    }
}
