using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of a database used by an analysis.
    /// </summary>
    public enum AnalysisDatabaseType
    {
        /// <summary>
        /// Represents an analysis database of no particular type.
        /// </summary>
        [Display(Name = "Node", Description = "The database has no particular type.")]
        None
    }
}
