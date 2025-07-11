using System;
using System.Collections.Generic;

namespace QKP.EzId.SourceGenerator
{
    /// <summary>
    /// Processes templates by replacing placeholders with actual values.
    /// </summary>
    internal class TemplateProcessor
    {
        private readonly string _templateContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateProcessor"/> class.
        /// </summary>
        /// <param name="templateContent">The template content.</param>
        public TemplateProcessor(string templateContent)
        {
            _templateContent = templateContent;
        }

        /// <summary>
        /// Loads a template by name.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <returns>A <see cref="TemplateProcessor"/> instance for the loaded template.</returns>
        public static TemplateProcessor LoadTemplate(string templateName)
        {
            string templateContent = templateName switch
            {
                nameof(Templates.EzIdImplementationTemplate) => Templates.EzIdImplementationTemplate,
                nameof(Templates.CompactEzIdTypeImplementationTemplate) => Templates.CompactEzIdTypeImplementationTemplate,
                nameof(Templates.JsonConverterImplementationTemplate) => Templates.JsonConverterImplementationTemplate,
                _ => throw new InvalidOperationException($"Unknown template: {templateName}")
            };

            return new TemplateProcessor(templateContent);
        }

        /// <summary>
        /// Processes the template by replacing placeholders with actual values.
        /// </summary>
        /// <param name="replacements">A dictionary of placeholder names and their replacement values.</param>
        /// <returns>The processed template content.</returns>
        public string Process(Dictionary<string, string> replacements)
        {
            string result = _templateContent;

            foreach (var replacement in replacements)
            {
                result = result.Replace($"{{{replacement.Key}}}", replacement.Value);
            }

            return result;
        }
    }
}
