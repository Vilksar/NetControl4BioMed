using NetControl4BioMed.Data.Interfaces;
using System;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between an analysis and a registered user that has access to it.
    /// </summary>
    public class AnalysisUser : IAnalysisDependent, IUserDependent
    {
        /// <summary>
        /// Gets or sets the date when the relationship was created.
        /// </summary>
        public DateTime DateTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the analysis ID of the relationship.
        /// </summary>
        public string AnalysisId { get; set; }

        /// <summary>
        /// Gets or sets the analysis of the relationship.
        /// </summary>
        public Analysis Analysis { get; set; }

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
