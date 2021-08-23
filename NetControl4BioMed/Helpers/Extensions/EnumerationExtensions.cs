using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

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

        /// <summary>
        /// Returns the enumeration value for the provided string.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="stringValue">The string which represents an enumeration value.</param>
        /// <returns></returns>
        public static T GetEnumerationValue<T>(string stringValue) where T : struct, IConvertible
        {
            // Check if the type is invalid.
            if (!typeof(T).IsEnum)
            {
                // Throw an exception.
                throw new ArgumentException("The provided type is not an enumeration.");
            }
            // Try to parse the string.
            if (!Enum.TryParse(stringValue, out T value))
            {
                // Throw an exception.
                throw new ArgumentException("The provided string is not valid for the enumeration.");
            }
            // Return the value.
            return value;
        }
    }
}
