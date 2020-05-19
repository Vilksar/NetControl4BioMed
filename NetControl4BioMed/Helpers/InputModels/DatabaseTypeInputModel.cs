using System.Collections;
using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a database type.
    /// </summary>
    public class DatabaseTypeInputModel
    {
        /// <summary>
        /// Represents the ID of the database type.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the database type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the database type.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the databases of the database type.
        /// </summary>
        public IEnumerable<DatabaseInputModel> Databases { get; set; }
    }
}
