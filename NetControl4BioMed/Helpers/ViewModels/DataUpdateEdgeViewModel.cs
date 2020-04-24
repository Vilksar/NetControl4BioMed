using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model for updating an edge read as JSON.
    /// </summary>
    public class DataUpdateEdgeViewModel
    {
        /// <summary>
        /// Represents the ID of the edge.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the description of the edge.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the IDs of the databases to be assigned to the edge.
        /// </summary>
        public IEnumerable<string> DatabaseIds { get; set; }

        /// <summary>
        /// Represents the nodes to be assigned to the edge.
        /// </summary>
        public IEnumerable<NodeModel> Nodes { get; set; }

        /// <summary>
        /// Represents the fields to be assigned to the edge.
        /// </summary>
        public IEnumerable<FieldModel> Fields { get; set; }

        /// <summary>
        /// Represents the model of a node assigned to the edge.
        /// </summary>
        public class NodeModel
        {
            /// <summary>
            /// Represents the ID of the node.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Represents the type of the node.
            /// </summary>
            public string Type { get; set; }
        }

        /// <summary>
        /// Represents the model of a field assigned to the edge.
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
        public static DataUpdateEdgeViewModel Default = new DataUpdateEdgeViewModel
        {
            Id = "Edge ID",
            Description = "Edge description",
            DatabaseIds = new List<string> { "Database ID" },
            Nodes = new List<NodeModel>
                {
                    new NodeModel
                    {
                        Id = "Source node ID",
                        Type = "Source"
                    },
                    new NodeModel
                    {
                        Id = "Target node ID",
                        Type = "Target"
                    }
                },
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
