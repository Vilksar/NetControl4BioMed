namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a network user.
    /// </summary>
    public class NetworkUserInputModel
    {
        /// <summary>
        /// Represents the network of the network user.
        /// </summary>
        public NetworkInputModel Network { get; set; }

        /// <summary>
        /// Represents the user of the network user.
        /// </summary>
        public UserInputModel User { get; set; }

        /// <summary>
        /// Represents the e-mail of the network user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Represents the type of the network user.
        /// </summary>
        public string Type { get; set; }
    }
}
