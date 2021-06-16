using NetControl4BioMed.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a network and a registered user who has access to it.
    /// </summary>
    public class NetworkUser : INetworkDependent, IUserDependent
    {
        /// <summary>
        /// Gets or sets the date when the relationship was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the network ID of the relationship.
        /// </summary>
        public string NetworkId { get; set; }

        /// <summary>
        /// Gets or sets the network of the relationship.
        /// </summary>
        public Network Network { get; set; }

        /// <summary>
        /// Gets or sets the user ID of the relationship.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the user of the relationship.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the e-mail of the relationship.
        /// </summary>
        public string Email { get; set; }
    }
}
