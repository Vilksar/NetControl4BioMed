using NetControl4BioMed.Data.Interfaces;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a database protein field and a protein with a corresponding value.
    /// </summary>
    public class DatabaseProteinFieldProtein : IProteinDependent
    {
        /// <summary>
        /// Gets or sets the database protein field ID of the relationship.
        /// </summary>
        public string DatabaseProteinFieldId { get; set; }

        /// <summary>
        /// Gets or sets the database protein field of the relationship.
        /// </summary>
        public DatabaseProteinField DatabaseProteinField { get; set; }

        /// <summary>
        /// Gets or sets the protein ID of the relationship.
        /// </summary>
        public string ProteinId { get; set; }

        /// <summary>
        /// Gets or sets the protein of the relationship.
        /// </summary>
        public Protein Protein { get; set; }

        /// <summary>
        /// Gets or sets the value of the relationship.
        /// </summary>
        public string Value { get; set; }
    }
}
