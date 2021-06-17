using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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

        /// <summary>
        /// Generates a random password based on the provided password options.
        /// </summary>
        /// <param name="passwordOptions">The options for the generated password.</param>
        /// <returns>A random password based on the provided password options.</returns>
        public static string GenerateRandomPassword(PasswordOptions passwordOptions)
        {
            // Check if there have been no password options provided.
            if (passwordOptions == null)
            {
                // Throw an exception.
                throw new ArgumentNullException(nameof(passwordOptions));
            }
            // Define the characters to choose from.
            var availableCharactersDictionary = new Dictionary<string, List<char>>
            {
                { "Lowercase", "abcdefghijkmnopqrstuvwxyz".ToList() },
                { "Uppercase", "ABCDEFGHJKLMNOPQRSTUVWXYZ".ToList() },
                { "Digit", "0123456789".ToList() },
                { "NonAlphaNumeric", ".,:;!@$?_-".ToList() }
            };
            // Get all of the available characters.
            var availableCharacters = availableCharactersDictionary.Values
                .SelectMany(item => item)
                .ToList();
            // Get the number of available characters.
            var availableCharactersCount = availableCharacters
                .Count();
            // Check if there are enough characters available.
            if (availableCharactersCount < passwordOptions.RequiredUniqueChars)
            {
                // Throw an exception.
                throw new ArgumentOutOfRangeException($"{nameof(passwordOptions)}.{nameof(passwordOptions.RequiredUniqueChars)}");
            }
            // Define a new random variable.
            var random = new Random();
            // Generate the bulk of the password with random characters from all available.
            var passwordCharacterList = Enumerable.Range(0, passwordOptions.RequiredLength)
                .Select(item => availableCharacters.GetRandomElement(random))
                .ToList();
            // Check if a lowercase character is required and doesn't already exist.
            if (passwordOptions.RequireLowercase && !passwordCharacterList.Intersect(availableCharactersDictionary["Lowercase"]).Any())
            {
                // Add a random lowercase character to the password.
                passwordCharacterList.Insert(random.Next(passwordCharacterList.Count()), availableCharactersDictionary["Lowercase"].GetRandomElement(random));
            }
            // Check if an uppercase character is required and doesn't already exist.
            if (passwordOptions.RequireUppercase && !passwordCharacterList.Intersect(availableCharactersDictionary["Uppercase"]).Any())
            {
                // Add a random uppercase character to the password.
                passwordCharacterList.Insert(random.Next(passwordCharacterList.Count()), availableCharactersDictionary["Uppercase"].GetRandomElement(random));
            }
            // Check if a digit character is required and doesn't already exist.
            if (passwordOptions.RequireDigit && !passwordCharacterList.Intersect(availableCharactersDictionary["Digit"]).Any())
            {
                // Add a random digit character to the password.
                passwordCharacterList.Insert(random.Next(passwordCharacterList.Count()), availableCharactersDictionary["Digit"].GetRandomElement(random));
            }
            // Check if a non-alphanumeric character is required and doesn't already exist.
            if (passwordOptions.RequireNonAlphanumeric && !passwordCharacterList.Intersect(availableCharactersDictionary["NonAlphaNumeric"]).Any())
            {
                // Add a random non-alphanumeric character to the password.
                passwordCharacterList.Insert(random.Next(passwordCharacterList.Count()), availableCharactersDictionary["NonAlphaNumeric"].GetRandomElement(random));
            }
            // Get the unique characters in the password.
            var uniqueCharacters = passwordCharacterList.Distinct();
            // Check if there aren't enough unique characters.
            if (uniqueCharacters.Count() < passwordOptions.RequiredUniqueChars)
            {
                // Get the unused characters to be added to the password.
                var unusedCharacters = availableCharacters
                    .Except(uniqueCharacters)
                    .OrderBy(item => random.NextDouble())
                    .Take(passwordOptions.RequiredUniqueChars - uniqueCharacters.Count());
                // Go over each unused character.
                foreach (var unusedCharacter in unusedCharacters)
                {
                    // Add it to the end of the password.
                    passwordCharacterList.Add(unusedCharacter);
                }
            }
            // Return the password.
            return new string(passwordCharacterList.ToArray());
        }
    }
}
