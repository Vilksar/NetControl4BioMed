using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model of a JSON file to be loaded in Cytoscape.
    /// </summary>
    public class CytoscapeViewModel
    {
        /// <summary>
        /// Represents the data of the Cytoscape file.
        /// </summary>
        [JsonPropertyName("data")]
        public CytoscapeData Data { get; set; }

        /// <summary>
        /// Represents the model of the data of the Cytoscape file.
        /// </summary>
        public class CytoscapeData
        {
            /// <summary>
            /// Represents the elements of the data.
            /// </summary>
            [JsonPropertyName("elements")]
            public CytoscapeElements Elements { get; set; }

            /// <summary>
            /// Represents the layout of the data.
            /// </summary>
            [JsonPropertyName("layout")]
            public CytoscapeLayout Layout { get; set; }

            /// <summary>
            /// Represents the style of the data.
            /// </summary>
            [JsonPropertyName("style")]
            public IEnumerable<CytoscapeStyle> Styles { get; set; }

            /// <summary>
            /// Represents the model of the elements of the data.
            /// </summary>
            public class CytoscapeElements
            {
                /// <summary>
                /// Represents the node elements.
                /// </summary>
                [JsonPropertyName("nodes")]
                public IEnumerable<CytoscapeNode> Nodes { get; set; }

                /// <summary>
                /// Represents the edge elements.
                /// </summary>
                [JsonPropertyName("edges")]
                public IEnumerable<CytoscapeEdge> Edges { get; set; }

                /// <summary>
                /// Represents the model of a node element.
                /// </summary>
                public class CytoscapeNode
                {
                    /// <summary>
                    /// Represents the data of the node.
                    /// </summary>
                    [JsonPropertyName("data")]
                    public CytoscapeNodeData Data { get; set; }

                    /// <summary>
                    /// Represents the classes of the node.
                    /// </summary>
                    [JsonPropertyName("classes")]
                    public IEnumerable<string> Classes { get; set; }

                    /// <summary>
                    /// Represents the model of the data.
                    /// </summary>
                    public class CytoscapeNodeData
                    {
                        /// <summary>
                        /// Represents the ID of the node.
                        /// </summary>
                        [JsonPropertyName("id")]
                        public string Id { get; set; }

                        /// <summary>
                        /// Represents the name of the node.
                        /// </summary>
                        [JsonPropertyName("name")]
                        public string Name { get; set; }

                        /// <summary>
                        /// Represents the link destination of the node.
                        /// </summary>
                        [JsonPropertyName("href")]
                        public string Href { get; set; }

                        /// <summary>
                        /// Represents the aliases of the node.
                        /// </summary>
                        [JsonPropertyName("alias")]
                        public IEnumerable<string> Alias { get; set; }
                    }
                }

                /// <summary>
                /// Represents the model of an edge element.
                /// </summary>
                public class CytoscapeEdge
                {
                    /// <summary>
                    /// Represents the data of the edge.
                    /// </summary>
                    [JsonPropertyName("data")]
                    public CytoscapeEdgeData Data { get; set; }

                    /// <summary>
                    /// Represents the classes of the edge.
                    /// </summary>
                    [JsonPropertyName("classes")]
                    public IEnumerable<string> Classes { get; set; }

                    /// <summary>
                    /// Represents the model of the data.
                    /// </summary>
                    public class CytoscapeEdgeData
                    {
                        /// <summary>
                        /// Represents the ID of the edge.
                        /// </summary>
                        [JsonPropertyName("id")]
                        public string Id { get; set; }

                        /// <summary>
                        /// Represents the name of the edge.
                        /// </summary>
                        [JsonPropertyName("name")]
                        public string Name { get; set; }

                        /// <summary>
                        /// Represents the ID of the source node of the edge.
                        /// </summary>
                        [JsonPropertyName("source")]
                        public string Source { get; set; }

                        /// <summary>
                        /// Represents the ID of the target node if the edge.
                        /// </summary>
                        [JsonPropertyName("target")]
                        public string Target { get; set; }

                        /// <summary>
                        /// Represents the type of the edge.
                        /// </summary>
                        [JsonPropertyName("interaction")]
                        public string Interaction { get; set; }
                    }
                }
            }

            /// <summary>
            /// Represents the model of the layout of the data.
            /// </summary>
            public class CytoscapeLayout
            {
                /// <summary>
                /// Represents the name of the layout.
                /// </summary>
                [JsonPropertyName("name")]
                public string Name { get; set; }
            }

            /// <summary>
            /// Represents the model of a style of the data.
            /// </summary>
            public class CytoscapeStyle
            {
                /// <summary>
                /// Represents the selector of the style.
                /// </summary>
                [JsonPropertyName("selector")]
                public string Selector { get; set; }

                /// <summary>
                /// Represents the CSS of the style.
                /// </summary>
                [JsonPropertyName("css")]
                public CytoscapeCSS Css { get; set; }

                /// <summary>
                /// Represents the model for the CSS of the style.
                /// </summary>
                public class CytoscapeCSS
                {
                    /// <summary>
                    /// Represents the text of an element.
                    /// </summary>
                    [JsonPropertyName("content")]
                    public string Content { get; set; }

                    /// <summary>
                    /// Represents the arrow shape of an element.
                    /// </summary>
                    [JsonPropertyName("mid-target-arrow-shape")]
                    public string MidTargetArrowShape { get; set; }

                    /// <summary>
                    /// Represents the color of an element.
                    /// </summary>
                    [JsonPropertyName("color")]
                    public string Color { get; set; }

                    /// <summary>
                    /// Represents the line color of an element.
                    /// </summary>
                    [JsonPropertyName("line-color")]
                    public string LineColor { get; set; }

                    /// <summary>
                    /// Represents the background color of an element.
                    /// </summary>
                    [JsonPropertyName("background-color")]
                    public string BackgroundColor { get; set; }
                }
            }
        }
    }
}
