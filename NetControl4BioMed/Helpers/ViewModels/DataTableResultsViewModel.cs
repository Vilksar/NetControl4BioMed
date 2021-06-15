using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model of a JSON file to be provided back to DataTables.
    /// </summary>
    public class DataTableResultsViewModel
    {
        /// <summary>
        /// Represents the data to be returned.
        /// </summary>
        [JsonPropertyName("data")]
        public List<DataTableResultItemViewModel> Data { get; set; }

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

        /// <summary>
        /// Represents the model of a single item within the result data.
        /// </summary>
        public class DataTableResultItemViewModel
        {
            /// <summary>
            /// Represents the ID of the item.
            /// </summary>
            [JsonPropertyName("id")]
            public string Id { get; set; }

            /// <summary>
            /// Represents the name of the item.
            /// </summary>
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }
    }
}
