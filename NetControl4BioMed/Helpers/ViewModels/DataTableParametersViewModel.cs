using Microsoft.AspNetCore.Mvc;
using NetControl4BioMed.Helpers.ModelBinders;
using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model of a JSON file to be provided by DataTables.
    /// </summary>
    [ModelBinder(BinderType = typeof(DataTableParametersModelBinder))]
    public class DataTableParametersViewModel
    {
        /// <summary>
        /// Represents the draw parameter.
        /// </summary>
        public int Draw { get; set; }

        /// <summary>
        /// Represents the start parameter.
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// Represents the length parameter.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Represents the selected items parameter.
        /// </summary>
        public List<string> SelectedItems { get; set; }

        /// <summary>
        /// Represents the search details.
        /// </summary>
        public DataTableSearchViewModel Search { get; set; }

        /// <summary>
        /// Represents the list of column orders.
        /// </summary>
        public List<DataTableColumnOrderViewModel> Order { get; set; }

        /// <summary>
        /// Represents the list of columns.
        /// </summary>
        public List<DataTableColumnViewModel> Columns { get; set; }

        /// <summary>
        /// Represents the model of a search.
        /// </summary>
        public class DataTableSearchViewModel
        {
            /// <summary>
            /// Represents the value to search for.
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Represents the flag indicating if the provided search value is RegEx.
            /// </summary>
            public bool Regex { get; set; }
        }

        /// <summary>
        /// Represents the model of a column.
        /// </summary>
        public class DataTableColumnViewModel
        {
            /// <summary>
            /// Represents the data of the column.
            /// </summary>
            public string Data { get; set; }

            /// <summary>
            /// Represents the name of the column.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Represents the searchable status of the column.
            /// </summary>
            public bool Searchable { get; set; }

            /// <summary>
            /// Represents the ordereable status of the column.
            /// </summary>
            public bool Orderable { get; set; }

            /// <summary>
            /// Represents the search details.
            /// </summary>
            public DataTableSearchViewModel Search { get; set; }
        }

        /// <summary>
        /// Represents the model of a column order.
        /// </summary>
        public class DataTableColumnOrderViewModel
        {
            /// <summary>
            /// Represents the column by which to order.
            /// </summary>
            public int Column { get; set; }

            /// <summary>
            /// Represents the direction in which to order.
            /// </summary>
            public string Direction { get; set; }
        }
    }
}
