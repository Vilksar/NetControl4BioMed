using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a sample.
    /// </summary>
    public class SampleInputModel
    {
        /// <summary>
        /// Represents the ID of the sample.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the sample.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the description of the sample.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the type of the sample.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Represents the data of the sample.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Represents the sample types of the sample.
        /// </summary>
        public IEnumerable<SampleTypeInputModel> SampleTypes { get; set; }

        /// <summary>
        /// Represents the sample databases of the sample.
        /// </summary>
        public IEnumerable<SampleDatabaseInputModel> SampleDatabases { get; set; }
    }
}
