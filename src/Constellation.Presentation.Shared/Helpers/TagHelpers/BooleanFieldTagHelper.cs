namespace Constellation.Presentation.Shared.Helpers.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;

public class BooleanFieldTagHelper : TagHelper
{
    public bool Value { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"badge text-bg-{(Value ? "success" : "danger")}");
        output.Content.SetContent((Value ? "Yes" : "No"));
    }
}