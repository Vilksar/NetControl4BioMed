using System;
using System.Runtime.Serialization;
using System.Text.Json;

namespace NetControl4BioMed.Helpers.Exceptions
{
    /// <summary>
    /// Implements an exception for the tasks.
    /// </summary>
    [Serializable]
    public class TaskException : Exception
    {
        /// <summary>
        /// Represents the JSON serializer options.
        /// </summary>
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        /// <summary>
        /// Initializes a new instance of the exception.
        /// </summary>
        public TaskException() : base() { }

        /// <summary>
        /// Initializes a new instance of the exception.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public TaskException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="item"></param>
        public TaskException(string message, bool showExceptionItem, object item) : base(GetMessage(message, showExceptionItem, item)) { }

        /// <summary>
        /// Initializes a new instance of the exception.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="innerException">The inner exception of the exception.</param>
        public TaskException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the exception.
        /// </summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <param name="streamingContext">The streaming context.</param>
        protected TaskException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

        /// <summary>
        /// Formats the provided message and object into a JSON string.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="item">The item that caused the exception.</param>
        /// <returns></returns>
        private static string GetMessage(string message, bool showExceptionItem, object item)
        {
            // Check if the exception item should be shown.
            if (showExceptionItem)
            {
                // Update the message.
                message += (string.IsNullOrEmpty(message) ? string.Empty : " ") +
                    "The item that triggered the exception will be displayed below." +
                    Environment.NewLine +
                    Environment.NewLine +
                    JsonSerializer.Serialize(item, _jsonSerializerOptions);
            }
            // Return the message.
            return message;
        }
    }
}
