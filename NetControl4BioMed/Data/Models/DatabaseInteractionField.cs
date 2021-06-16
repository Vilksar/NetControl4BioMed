using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a database interaction field corresponding to a database.
    /// </summary>
    public class DatabaseInteractionField : IDatabaseDependent
    {
        /// <summary>
        /// Gets or sets the unique internal ID of the database interaction field.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the database interaction field was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the ID of the database containing the database interaction field.
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Gets or sets the database containing the database interaction field.
        /// </summary>
        public Database Database { get; set; }

        /// <summary>
        /// Gets or sets the name of the database interaction field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the database interaction field.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the URL for the database interaction field. The variable in the query will be replaced with the corresponding value.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the automatic detection state for searches of the interactions in the database interaction field.
        /// </summary>
        public bool IsSearchable { get; set; }

        /// <summary>
        /// Gets or sets the interactions which have a value in the database interaction field.
        /// </summary>
        public ICollection<DatabaseInteractionFieldInteraction> DatabaseInteractionFieldInteractions { get; set; }
    }
}
