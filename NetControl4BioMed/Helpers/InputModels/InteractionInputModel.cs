using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for an interaction.
    /// </summary>
    public class InteractionInputModel
    {
        /// <summary>
        /// Represents the ID of the interaction.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the interaction.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the interaction.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the database interactions of the interaction.
        /// </summary>
        public IEnumerable<DatabaseInteractionInputModel> DatabaseInteractions { get; set; }

        /// <summary>
        /// Represents the database interaction field interactions of the interaction.
        /// </summary>
        public IEnumerable<DatabaseInteractionFieldInteractionInputModel> DatabaseInteractionFieldInteractions { get; set; }

        /// <summary>
        /// Represents the interaction proteins of the interaction.
        /// </summary>
        public IEnumerable<InteractionProteinInputModel> InteractionProteins { get; set; }

        /// <summary>
        /// Represents the network interactions of the interaction.
        /// </summary>
        public IEnumerable<NetworkInteractionInputModel> NetworkInteractions { get; set; }

        /// <summary>
        /// Represents the analysis interactions of the interaction.
        /// </summary>
        public IEnumerable<AnalysisInteractionInputModel> AnalysisInteractions { get; set; }

        /// <summary>
        /// Represents the path interactions of the interaction.
        /// </summary>
        public ICollection<PathInteractionInputModel> PathInteractions { get; set; }
    }
}
