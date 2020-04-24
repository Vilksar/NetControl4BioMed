using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model for updating a node read as JSON.
    /// </summary>
    public class DataUpdateNodeViewModel
    {
        /// <summary>
        /// Represents the ID of the node.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the description of the node.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the fields to be assigned to the node.
        /// </summary>
        public IEnumerable<FieldModel> Fields { get; set; }

        /// <summary>
        /// Represents the model of a field assigned to the node.
        /// </summary>
        public class FieldModel
        {
            /// <summary>
            /// Represents the ID of the field.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// Represents the value of the node corresponding to the field.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Represents the default value.
        /// </summary>
        public static DataUpdateNodeViewModel Default = new DataUpdateNodeViewModel
        {
            Id = "Node ID",
            Description = "Node description",
            Fields = new List<FieldModel>
                {
                    new FieldModel
                    {
                        Key = "Field ID",
                        Value = "Value"
                    }
                }
        };
    }
}
