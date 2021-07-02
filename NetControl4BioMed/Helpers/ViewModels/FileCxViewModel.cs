using System.Collections.Generic;
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
            /// Represents the edges of the data.
            /// </summary>
            [JsonPropertyName("edges")]
            public IEnumerable<CxEdge> Edges { get; set; }

            /// <summary>
            /// Represents the status of the data.
            /// </summary>
            [JsonPropertyName("status")]
            public IEnumerable<CxStatus> Status { get; set; }

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
    }
}
