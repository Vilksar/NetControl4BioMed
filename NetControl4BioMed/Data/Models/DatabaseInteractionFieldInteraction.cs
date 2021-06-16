using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a database interaction field and an interaction with a corresponding value.
    /// </summary>
    public class DatabaseInteractionFieldInteraction : IInteractionDependent
    {
        /// <summary>
        /// Gets or sets the database interaction field ID of the relationship.
        /// </summary>
        public string DatabaseInteractionFieldId { get; set; }

        /// <summary>
        /// Gets or sets the database interaction field of the relationship.
        /// </summary>
        public DatabaseInteractionField DatabaseInteractionField { get; set; }

        /// <summary>
        /// Gets or sets the interaction ID of the relationship.
        /// </summary>
        public string InteractionId { get; set; }

        /// <summary>
        /// Gets or sets the interaction of the relationship.
        /// </summary>
        public Interaction Interaction { get; set; }

        /// <summary>
        /// Gets or sets the value of the relationship.
        /// </summary>
        public string Value { get; set; }
    }
}
