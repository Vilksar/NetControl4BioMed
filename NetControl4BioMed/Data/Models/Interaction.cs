using System;
using System.Collections.Generic;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of an interaction.
    /// </summary>
    public class Interaction
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the interaction.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the interaction has been created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the name of the interaction.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the interaction.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the databases in which the interaction appears.
        /// </summary>
        public ICollection<DatabaseInteraction> DatabaseInteractions { get; set; }

        /// <summary>
        /// Gets or sets the interaction database fields which have a value corresponding to the interaction.
        /// </summary>
        public ICollection<DatabaseInteractionFieldInteraction> DatabaseInteractionFieldInteractions { get; set; }

        /// <summary>
        /// Gets or sets the proteins which appear in the interaction.
        /// </summary>
        public ICollection<InteractionProtein> InteractionProteins { get; set; }

        /// <summary>
        /// Gets or sets the networks which contain the interaction.
        /// </summary>
        public ICollection<NetworkInteraction> NetworkInteractions { get; set; }

        /// <summary>
        /// Gets or sets the analyses which contain the interaction.
        /// </summary>
        public ICollection<AnalysisInteraction> AnalysisInteractions { get; set; }

        /// <summary>
        /// Gets or sets the paths which contain the interaction.
        /// </summary>
        public ICollection<PathInteraction> PathInteractions { get; set; }
    }
}
