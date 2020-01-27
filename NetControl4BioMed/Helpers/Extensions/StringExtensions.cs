using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Tries to deserialize the given JSON string as the specified type.
        /// </summary>
        /// <typeparam name="T">Represents the type of the JSON object.</typeparam>
        /// <param name="jsonString">Represents a JSON string.</param>
        /// <returns>Returns the object of type T if the deserialization was successful, null otherwise.</returns>
        public static bool TryDeserializeJsonObject<T>(this string jsonString, out T value)
        {
            // Try to deserialize the given string.
            try
            {
                // Get the deserialized object and assign the output value.
                value = JsonSerializer.Deserialize<T>(jsonString);
                // Return it.
                return true;
            }
            catch (Exception)
            {
                // Assign the default value.
                value = default;
                // Return null.
                return false;
            }
        }
    }
}
