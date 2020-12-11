using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a database and an unregistered user that will have access to it after registration, even when it is not public.
    /// </summary>
    public class DatabaseUserInvitation : IDatabaseDependent
    {
        /// <summary>
        /// Gets or sets the date when the relationship was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the database ID of the relationship.
        /// </summary>
        public string DatabaseId { get; set; }

        /// <summary>
        /// Gets or sets the database of the relationship.
        /// </summary>
        public Database Database { get; set; }

        /// <summary>
        /// Gets or sets the e-mail of the relationship.
        /// </summary>
        public string Email { get; set; }
    }
}
