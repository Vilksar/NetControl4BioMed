using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a role.
    /// </summary>
    public class Role : IdentityRole
    {
        /// <summary>
        /// Gets or sets the date when the role was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the users which are assigned to the role.
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
