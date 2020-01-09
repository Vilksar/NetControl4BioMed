using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Enumerations
{
    /// <summary>
    /// Represents the possible types of an edge database field.
    /// </summary>
    public enum DatabaseEdgeFieldType
    {
        /// <summary>
        /// Represents a boolean.
        /// </summary>
        [Display(Name = "Boolean", Description = "The values in this field are boolean.")]
        Boolean,

        /// <summary>
        /// Represents a real number (double).
        /// </summary>
        [Display(Name = "Number", Description = "The values in this field are real numbers.")]
        Number,

        /// <summary>
        /// Represents a string.
        /// </summary>
        [Display(Name = "String", Description = "The values in this field are strings.")]
        String
    }
}
