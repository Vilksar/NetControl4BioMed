using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model of a JSON file to be integrated with NDEx.
    /// </summary>
    public class FileCxViewModel
    {
        /// <summary>
        /// Represents the base object of the data.
        /// </summary>
        public class CxBaseObject
        {
            /// <summary>
            /// Represents the meta data of the data.
            /// </summary>
            [JsonPropertyName("metaData")]
            public IEnumerable<CxMetaData> MetaData { get; set; }

            /// <summary>
            /// Represents the network attributes of the data.
            /// </summary>
            [JsonPropertyName("networkAttributes")]
            public IEnumerable<CxNetworkAttribute> NetworkAttributes { get; set; }

            /// <summary>
            /// Represents the nodes of the data.
            /// </summary>
            [JsonPropertyName("nodes")]
            public IEnumerable<CxNode> Nodes { get; set; }

            /// <summary>
            /// Represents the node attributes of the data.
            /// </summary>
            [JsonPropertyName("nodeAttributes")]
            public IEnumerable<CxNodeAttribute> NodeAttributes { get; set; }

            /// <summary>
            /// Represents the edges of the data.
            /// </summary>
            [JsonPropertyName("edges")]
            public IEnumerable<CxEdge> Edges { get; set; }

            /// <summary>
            /// Represents the edge attributes of the data.
            /// </summary>
            [JsonPropertyName("edgeAttributes")]
            public IEnumerable<CxEdgeAttribute> EdgeAttributes { get; set; }

            /// <summary>
            /// Represents the table columns for Cytoscape.
            /// </summary>
            [JsonPropertyName("cyTableColumn")]
            public IEnumerable<CxCytoscapeTableColumn> CytoscapeTableColumns { get; set; }

            /// <summary>
            /// Represents the status of the data.
            /// </summary>
            [JsonPropertyName("status")]
            public IEnumerable<CxStatus> Status { get; set; }

            /// <summary>
            /// Represents the model of a meta data.
            /// </summary>
            public class CxMetaData
            {
                /// <summary>
                /// Represents the name of the meta data.
                /// </summary>
                [JsonPropertyName("name")]
                public string Name { get; set; }

                /// <summary>
                /// Represents the element count of the meta data.
                /// </summary>
                [JsonPropertyName("elementCount")]
                public int ElementCount { get; set; }

                /// <summary>
                /// Represents the version of the meta data.
                /// </summary>
                [JsonPropertyName("version")]
                public string Version { get; set; }

                /// <summary>
                /// Represents the maximum ID of the elements of the meta data.
                /// </summary>
                [JsonPropertyName("idCounter")]
                public int IdCounter { get; set; }
            }

            /// <summary>
            /// Represents the model of a network attribute.
            /// </summary>
            public class CxNetworkAttribute
            {
                /// <summary>
                /// Represents the name of the attribute.
                /// </summary>
                [JsonPropertyName("n")]
                public string Name { get; set; }

                /// <summary>
                /// Represents the value of the attribute.
                /// </summary>
                [JsonPropertyName("v")]
                public string Value { get; set; }
            }

            /// <summary>
            /// Represents the model of a node.
            /// </summary>
            public class CxNode
            {
                /// <summary>
                /// Represents the ID of the node.
                /// </summary>
                [JsonPropertyName("@id")]
                public int Id { get; set; }

                /// <summary>
                /// Represents the name of the node.
                /// </summary>
                [JsonPropertyName("n")]
                public string Name { get; set; }
            }

            /// <summary>
            /// Represents the model of a node attribute.
            /// </summary>
            public class CxNodeAttribute
            {
                /// <summary>
                /// Represents the ID of the current node corresponding to the attribute.
                /// </summary>
                [JsonPropertyName("po")]
                public int Id { get; set; }

                /// <summary>
                /// Represents the name of the attribute.
                /// </summary>
                [JsonPropertyName("n")]
                public string Name { get; set; }

                /// <summary>
                /// Represents the value of the attribute for the current node.
                /// </summary>
                [JsonPropertyName("v")]
                public string Value { get; set; }

                /// <summary>
                /// Represents the data type of the attribute.
                /// </summary>
                [JsonPropertyName("d")]
                public string Data { get; set; }
            }

            /// <summary>
            /// Represents the model of an edge.
            /// </summary>
            public class CxEdge
            {
                /// <summary>
                /// Represents the ID of the edge.
                /// </summary>
                [JsonPropertyName("@id")]
                public int Id { get; set; }

                /// <summary>
                /// Represents the ID of the source node of the edge.
                /// </summary>
                [JsonPropertyName("s")]
                public int Source { get; set; }

                /// <summary>
                /// Represents the ID of the target node of the edge.
                /// </summary>
                [JsonPropertyName("t")]
                public int Target { get; set; }

                /// <summary>
                /// Represents the type of the edge.
                /// </summary>
                [JsonPropertyName("i")]
                public string Type { get; set; }
            }

            /// <summary>
            /// Represents the model of an edge attribute.
            /// </summary>
            public class CxEdgeAttribute
            {
                /// <summary>
                /// Represents the ID of the current edge corresponding to the attribute.
                /// </summary>
                [JsonPropertyName("po")]
                public int Id { get; set; }

                /// <summary>
                /// Represents the name of the attribute.
                /// </summary>
                [JsonPropertyName("n")]
                public string Name { get; set; }

                /// <summary>
                /// Represents the value of the attribute for the current edge.
                /// </summary>
                [JsonPropertyName("v")]
                public string Value { get; set; }

                /// <summary>
                /// Represents the data type of the attribute.
                /// </summary>
                [JsonPropertyName("d")]
                public string Data { get; set; }
            }

            /// <summary>
            /// Represents the model of a table column for Cytoscape.
            /// </summary>
            public class CxCytoscapeTableColumn
            {
                /// <summary>
                /// Represents the table type of the column.
                /// </summary>
                [JsonPropertyName("applies_to")]
                public string AppliesTo { get; set; }

                /// <summary>
                /// Represents the name of the column.
                /// </summary>
                [JsonPropertyName("n")]
                public string Name { get; set; }

                /// <summary>
                /// Represents the data type of the column.
                /// </summary>
                [JsonPropertyName("d")]
                public string Data { get; set; }
            }

            /// <summary>
            /// Represents the model of a returned status.
            /// </summary>
            public class CxStatus
            {
                /// <summary>
                /// Represents the error message of the status.
                /// </summary>
                [JsonPropertyName("error")]
                public string ErrorMessage { get; set; }

                /// <summary>
                /// Represents the success flag of the status.
                /// </summary>
                [JsonPropertyName("success")]
                public bool IsSuccessful { get; set; }
            }
        }

        /// <summary>
        /// Updates the provided list of objects with the corresponding meta data.
        /// </summary>
        /// <param name="data"></param>
        public static void AddMetaData(List<CxBaseObject> data)
        {
            // Insert a new object in the beginning of the list.
            data.Insert(0, new CxBaseObject
            {
                MetaData = new List<CxBaseObject.CxMetaData>
                    {
                        new CxBaseObject.CxMetaData
                        {
                            Name = "networkAttributes",
                            ElementCount = data.FirstOrDefault(item => item.NetworkAttributes != null)?.NetworkAttributes.Count() ?? 0,
                            Version = "1.0"
                        },
                        new CxBaseObject.CxMetaData
                        {
                            Name = "nodes",
                            ElementCount = data.FirstOrDefault(item => item.Nodes != null)?.Nodes.Count() ?? 0,
                            IdCounter = data.FirstOrDefault(item => item.Nodes != null)?.Nodes.Max(item => item.Id) ?? 0,
                            Version = "1.0"
                        },
                        new CxBaseObject.CxMetaData
                        {
                            Name = "nodeAttributes",
                            ElementCount = data.FirstOrDefault(item => item.NodeAttributes != null)?.NodeAttributes.Count() ?? 0,
                            Version = "1.0"
                        },
                        new CxBaseObject.CxMetaData
                        {
                            Name = "edges",
                            ElementCount = data.FirstOrDefault(item => item.Edges != null)?.Edges.Count() ?? 0,
                            IdCounter = data.FirstOrDefault(item => item.Edges != null)?.Edges.Max(item => item.Id) ?? 0,
                            Version = "1.0"
                        },
                        new CxBaseObject.CxMetaData
                        {
                            Name = "edgeAttributes",
                            ElementCount = data.FirstOrDefault(item => item.EdgeAttributes != null)?.EdgeAttributes.Count() ?? 0,
                            Version = "1.0"
                        },
                        new CxBaseObject.CxMetaData
                        {
                            Name = "cyTableColumn",
                            ElementCount = data.FirstOrDefault(item => item.CytoscapeTableColumns != null)?.CytoscapeTableColumns.Count() ?? 0,
                            Version = "1.0"
                        }
                    }
            });
        }

        /// <summary>
        /// Gets the default meta data.
        /// </summary>
        public static IEnumerable<CxBaseObject.CxCytoscapeTableColumn> DefaultCytoscapeTableColumns { get; } = new List<CxBaseObject.CxCytoscapeTableColumn>
        {
            new CxBaseObject.CxCytoscapeTableColumn
            {
                AppliesTo = "network_table",
                Name = "name"
            },
            new CxBaseObject.CxCytoscapeTableColumn
            {
                AppliesTo = "network_table",
                Name = "shared name"
            },
            new CxBaseObject.CxCytoscapeTableColumn
            {
                AppliesTo = "network_table",
                Name = "description"
            },
            new CxBaseObject.CxCytoscapeTableColumn
            {
                AppliesTo = "node_table",
                Name = "name"
            },
            new CxBaseObject.CxCytoscapeTableColumn
            {
                AppliesTo = "node_table",
                Name = "shared name"
            },
            new CxBaseObject.CxCytoscapeTableColumn
            {
                AppliesTo = "node_table",
                Name = "type"
            },
            new CxBaseObject.CxCytoscapeTableColumn
            {
                AppliesTo = "edge_table",
                Name = "name"
            },
            new CxBaseObject.CxCytoscapeTableColumn
            {
                AppliesTo = "edge_table",
                Name = "shared name"
            },
            new CxBaseObject.CxCytoscapeTableColumn
            {
                AppliesTo = "edge_table",
                Name = "interaction"
            },
            new CxBaseObject.CxCytoscapeTableColumn
            {
                AppliesTo = "edge_table",
                Name = "shared interaction"
            }
        };
    }
}
