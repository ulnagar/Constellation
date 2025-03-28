namespace Constellation.Presentation.Shared.Helpers.TagHelpers;

using Core.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

public class GenderTagHelper : TagHelper
{
    public Gender Gender { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        string colour = Gender switch
        {
            not null when Gender.Equals(Gender.Female) => "glyph-color-pink",
            not null when Gender.Equals(Gender.Male) => "glyph-color-blue",
            not null when Gender.Equals(Gender.NonBinary) => "glyph-color-yellow",
            _ => "glyph-color-gray"
        };

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"glyphicon glyphicon-user {colour}");
        output.Attributes.SetAttribute("style", "padding-right: 5px;");
        output.Attributes.SetAttribute("title", Gender.Name);
    }
}