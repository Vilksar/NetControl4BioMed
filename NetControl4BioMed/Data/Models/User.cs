using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a user.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// Gets or sets the date when the user was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the roles which are assigned to the user.
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; }

        /// <summary>
        /// Gets or sets the databases to which the user has access.
        /// </summary>
        public ICollection<DatabaseUser> DatabaseUsers { get; set; }

        /// <summary>
        /// Gets or sets the networks for which the user is owner.
        /// </summary>
        public ICollection<Network> OwnedNetworks { get; set; }

        /// <summary>
        /// Gets or sets the networks to which the user has access.
        /// </summary>
        public ICollection<NetworkUser> NetworkUsers { get; set; }

        /// <summary>
        /// Gets or sets the analyses for which the user is owner.
        /// </summary>
        public ICollection<Analysis> OwnedAnalyses { get; set; }

        /// <summary>
        /// Gets or sets the analyses to which the user has access.
        /// </summary>
        public ICollection<AnalysisUser> AnalysisUsers { get; set; }
    }
}
