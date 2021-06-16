using NetControl4BioMed.Data.Models;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on a database.
    /// </summary>
    public interface IDatabaseDependent
    {
        /// <summary>
        /// Represents the database ID of the model.
        /// </summary>
        string DatabaseId { get; set; }

        /// <summary>
        /// Represents the database of the model.
        /// </summary>
        Database Database { get; set; }
    }
}
