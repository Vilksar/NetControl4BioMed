using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a user and a role to which it is assigned.
    /// </summary>
    public class UserRole : IdentityUserRole<string>
    {
        /// <summary>
        /// Gets or sets the user of the relationship.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the role of the relationship.
        /// </summary>
        public Role Role { get; set; }
    }
}
