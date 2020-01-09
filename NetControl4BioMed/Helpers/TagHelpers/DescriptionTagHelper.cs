using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.TagHelpers
{
    /// <summary>
    /// Provides an implementation for a "description" tag for a given element, based on its data annotation attributes.
    /// </summary>
    [HtmlTargetElement("span")]
    public class DescriptionTagHelper : TagHelper
    {
        /// <summary>
        /// The attribute name for the tag.
        /// </summary>
        private const string DescriptionForAttributeName = "asp-description-for";

        /// <summary>
        /// The model expression for the tag.
        /// </summary>
        [HtmlAttributeName(DescriptionForAttributeName)]
        public ModelExpression For { get; set; }

        /// <summary>
        /// Parses the given expression into an HTML tag.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Check if there isn't any "asp-description-for" attribute in the tag.
            if (For == null)
            {
                // Return.
                return;
            }
            // Check if there wasn't any metadata found.
            if (For.Metadata == null)
            {
                // Return.
                return;
            }
            // Get the child content of the output tag.
            var childContent = output.GetChildContentAsync().Result;
            // Check if there is already some content.
            if (!childContent.IsEmptyOrWhiteSpace)
            {
                // Keep unchanged the child content.
                output.Content.SetHtmlContent(childContent);
                // Return.
                return;
            }
            // Update the content of the tag.
            output.Content.SetHtmlContent(!string.IsNullOrEmpty(For.Metadata.Description) ? For.Metadata.Description : "No other information is available.");
        }
    }
}
