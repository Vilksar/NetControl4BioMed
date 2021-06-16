using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Interfaces
{
    /// <summary>
    /// Provides an abstraction for a model depending on a user.
    /// </summary>
    public interface IUserDependent
    {
        /// <summary>
        /// Represents the user ID of the model.
        /// </summary>
        string UserId { get; set; }

        /// <summary>
        /// Represents the user of the model.
        /// </summary>
        User User { get; set; }
    }
}
