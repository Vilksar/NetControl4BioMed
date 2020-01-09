using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for the enums.
    /// </summary>
    public static class EnumerationExtensions
    {
        /// <summary>
        /// Returns the defined display name for the current enumeration member.
        /// </summary>
        /// <param name="enumeration">The enumeration for which to get the display name.</param>
        /// <returns>The defined display name for the current enumeration.</returns>
        public static string GetDisplayName(this Enum enumeration)
        {
            // Return the defined display name for the current enumeration.
            return enumeration.GetType()
                .GetMember(enumeration.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                .Name;
        }

        /// <summary>
        /// Returns the defined display description for the current enumeration member.
        /// </summary>
        /// <param name="enumeration">The enumeration for which to get the display description.</param>
        /// <returns>The defined display description for the current enumeration.</returns>
        public static string GetDisplayDescription(this Enum enumeration)
        {
            // Return the defined display description for the current enumeration.
            return enumeration.GetType()
                .GetMember(enumeration.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                .Description;
        }
    }
}
