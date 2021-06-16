using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on an interaction.
    /// </summary>
    public interface IInteractionDependent
    {
        /// <summary>
        /// Represents the interaction ID of the model.
        /// </summary>
        string InteractionId { get; set; }

        /// <summary>
        /// Represents the interaction of the model.
        /// </summary>
        Interaction Interaction { get; set; }
    }
}
