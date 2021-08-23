using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetControl4BioMed.Helpers.ViewModels
{
    /// <summary>
    /// Represents the model of a JSON file to be rendered in Cytoscape and CytoscapeJS.
    /// </summary>
    public class FileCyjsViewModel
    {
        /// <summary>
        /// Represents the details of the data.
        /// </summary>
        [JsonPropertyName("data")]
        public CyjsData Data { get; set; }

        /// <summary>
        /// Represents the elements of the data.
        /// </summary>
        [JsonPropertyName("elements")]
        public CyjsElements Elements { get; set; }

        /// <summary>
        /// Represents the layout of the data.
        /// </summary>
        [JsonPropertyName("layout")]
        public CyjsLayout Layout { get; set; }

        /// <summary>
        /// Represents the style of the data.
        /// </summary>
        [JsonPropertyName("style")]
        public IEnumerable<CyjsStyle> Styles { get; set; }

        /// <summary>
        /// Represents the model of the data.
        /// </summary>
        public class CyjsData
        {
            /// <summary>
            /// Represents the ID of the data.
            /// </summary>
            [JsonPropertyName("id")]
            public string Id { get; set; }

            /// <summary>
            /// Represents the name of the data.
            /// </summary>
            [JsonPropertyName("name")]
            public string Name { get; set; }

            /// <summary>
            /// Represents the description of the data.
            /// </summary>
            [JsonPropertyName("description")]
            public string Description { get; set; }
        }

        /// <summary>
        /// Represents the model of the elements of the data.
        /// </summary>
        public class CyjsElements
        {
            /// <summary>
            /// Represents the node elements.
            /// </summary>
            [JsonPropertyName("nodes")]
            public IEnumerable<CyjsNode> Nodes { get; set; }

            /// <summary>
            /// Represents the edge elements.
            /// </summary>
            [JsonPropertyName("edges")]
            public IEnumerable<CyjsEdge> Edges { get; set; }

            /// <summary>
            /// Represents the model of a node element.
            /// </summary>
            public class CyjsNode
            {
                /// <summary>
                /// Represents the data of the node.
                /// </summary>
                [JsonPropertyName("data")]
                public CyjsNodeData Data { get; set; }

                /// <summary>
                /// Represents the classes of the node.
                /// </summary>
                [JsonPropertyName("classes")]
                public IEnumerable<string> Classes { get; set; }

                /// <summary>
                /// Represents the model of the data.
                /// </summary>
                public class CyjsNodeData
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
                    /// Represents the type of the node.
                    /// </summary>
                    [JsonPropertyName("type")]
                    public string Type { get; set; }
                }
            }

            /// <summary>
            /// Represents the model of an edge element.
            /// </summary>
            public class CyjsEdge
            {
                /// <summary>
                /// Represents the data of the edge.
                /// </summary>
                [JsonPropertyName("data")]
                public CyjsEdgeData Data { get; set; }

                /// <summary>
                /// Represents the classes of the edge.
                /// </summary>
                [JsonPropertyName("classes")]
                public IEnumerable<string> Classes { get; set; }

                /// <summary>
                /// Represents the model of the data.
                /// </summary>
                public class CyjsEdgeData
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
                    /// Represents the link destination of the edge.
                    /// </summary>
                    [JsonPropertyName("href")]
                    public string Href { get; set; }

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
                    [JsonPropertyName("type")]
                    public string Type { get; set; }
                }
            }
        }

        /// <summary>
        /// Represents the model of the layout of the data.
        /// </summary>
        public class CyjsLayout
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
        public class CyjsStyle
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
            public CyjsCSS Css { get; set; }

            /// <summary>
            /// Represents the model for the CSS of the style.
            /// </summary>
            public class CyjsCSS
            {
                /// <summary>
                /// Represents the text of an element.
                /// </summary>
                [JsonPropertyName("content")]
                public string Content { get; set; }

                /// <summary>
                /// Represents the arrow color of an element.
                /// </summary>
                [JsonPropertyName("target-arrow-color")]
                public string TargetArrowColor { get; set; }

                /// <summary>
                /// Represents the arrow shape of an element.
                /// </summary>
                [JsonPropertyName("target-arrow-shape")]
                public string TargetArrowShape { get; set; }

                /// <summary>
                /// Represents the line color of an element.
                /// </summary>
                [JsonPropertyName("line-color")]
                public string LineColor { get; set; }

                /// <summary>
                /// Represents the curve style of an element.
                /// </summary>
                [JsonPropertyName("curve-style")]
                public string CurveStyle { get; set; }

                /// <summary>
                /// Represents the background color of an element.
                /// </summary>
                [JsonPropertyName("background-color")]
                public string BackgroundColor { get; set; }
            }
        }

        /// <summary>
        /// Gets the default layout of the data.
        /// </summary>
        public static CyjsLayout DefaultLayout { get; } = new CyjsLayout
        {
            Name = "cose"
        };

        /// <summary>
        /// Gets the default style of the data.
        /// </summary>
        public static IEnumerable<CyjsStyle> DefaultStyles { get; } = new List<CyjsStyle>
        {
            new CyjsStyle
            {
                Selector = "node",
                Css = new CyjsStyle.CyjsCSS
                {
                    Content = "data(name)",
                    BackgroundColor = "lightgray"
                }
            },
            new CyjsStyle
            {
                Selector = "edge",
                Css = new CyjsStyle.CyjsCSS
                {
                    TargetArrowColor = "lightgray",
                    TargetArrowShape = "triangle",
                    LineColor = "lightgray",
                    CurveStyle = "bezier"
                }
            }
        };

        /// <summary>
        /// Gets the default style of the network-specific data.
        /// </summary>
        public static IEnumerable<CyjsStyle> DefaultNetworkStyles { get; } = new List<CyjsStyle>
        {
            new CyjsStyle
            {
                Selector = "node.seed",
                Css = new CyjsStyle.CyjsCSS
                {
                    BackgroundColor = "tomato"
                }
            }
        };

        /// <summary>
        /// Gets the default style of the analysis-specific data.
        /// </summary>
        public static IEnumerable<CyjsStyle> DefaultAnalysisStyles { get; } = new List<CyjsStyle>
        {
            new CyjsStyle
            {
                Selector = "node.source",
                Css = new CyjsStyle.CyjsCSS
                {
                    BackgroundColor = "lightpink"
                }
            },
            new CyjsStyle
            {
                Selector = "node.target",
                Css = new CyjsStyle.CyjsCSS
                {
                    BackgroundColor = "cornflowerblue"
                }
            },
            new CyjsStyle
            {
                Selector = "node.source.target",
                Css = new CyjsStyle.CyjsCSS
                {
                    BackgroundColor = "mediumpurple"
                }
            }
        };

        /// <summary>
        /// Gets the default style of the control-path-specific data.
        /// </summary>
        public static IEnumerable<CyjsStyle> DefaultControlPathStyles { get; } = new List<CyjsStyle>
        {
            new CyjsStyle
            {
                Selector = "node.control",
                Css = new CyjsStyle.CyjsCSS
                {
                    BackgroundColor = "forestgreen"
                }
            },
            new CyjsStyle
            {
                Selector = "node.source.control",
                Css = new CyjsStyle.CyjsCSS
                {
                    BackgroundColor = "deeppink"
                }
            },
            new CyjsStyle
            {
                Selector = "node.target.control",
                Css = new CyjsStyle.CyjsCSS
                {
                    BackgroundColor = "aquamarine"
                }
            },
            new CyjsStyle
            {
                Selector = "node.source.target.control",
                Css = new CyjsStyle.CyjsCSS
                {
                    BackgroundColor = "darkgoldenrod"
                }
            },
            new CyjsStyle
            {
                Selector = "edge.control",
                Css = new CyjsStyle.CyjsCSS
                {
                    TargetArrowColor = "black",
                    LineColor = "black"
                }
            },
        };
    }
}
