using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model of a DataTables response.
    /// </summary>
    public class DataTableResponseViewModel
    {
        /// <summary>
        /// Represents the data to be returned.
        /// </summary>
        [JsonPropertyName("data")]
        public List<List<string>> Data { get; set; }

        /// <summary>
        /// Represents the draw parameter.
        /// </summary>
        [JsonPropertyName("draw")]
        public int Draw { get; set; }

        /// <summary>
        /// Represents the number of filtered items.
        /// </summary>
        [JsonPropertyName("recordsFiltered")]
        public int RecordsFiltered { get; set; }

        /// <summary>
        /// Represents the total number of items.
        /// </summary>
        [JsonPropertyName("recordsTotal")]
        public int RecordsTotal { get; set; }
    }
}
