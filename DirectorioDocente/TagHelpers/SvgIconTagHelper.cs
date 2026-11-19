using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.IO;

namespace DirectorioDocente.TagHelpers
{
    [HtmlTargetElement("icon")]
    public class SvgIconTagHelper : TagHelper
    {
        private readonly IWebHostEnvironment _env;

        public SvgIconTagHelper(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HtmlAttributeName("name")]
        public string Name { get; set; } = string.Empty;

        [HtmlAttributeName("class")]
        public string? Class { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            if (string.IsNullOrWhiteSpace(Name)) return;

            var path = Path.Combine(_env.WebRootPath, "assets", "icons", $"{Name}.svg");

            if (!File.Exists(path))
            {
                output.SuppressOutput();
                return;
            }

            var svgContent = File.ReadAllText(path);

            if (svgContent.Contains("<svg"))
            {
                var extraAttrs = svgContent.Contains("aria-hidden") ? "" : " aria-hidden=\"true\" focusable=\"false\"";
                if (!string.IsNullOrWhiteSpace(Class))
                {
                    svgContent = svgContent.Replace("<svg", $"<svg{extraAttrs} class=\"{Class}\"");
                }
                else if (!string.IsNullOrEmpty(extraAttrs))
                {
                    svgContent = svgContent.Replace("<svg", $"<svg{extraAttrs}");
                }
            }

            output.Content.SetHtmlContent(svgContent);
        }
    }
}