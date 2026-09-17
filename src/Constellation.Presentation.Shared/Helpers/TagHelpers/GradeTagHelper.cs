namespace Constellation.Presentation.Shared.Helpers.TagHelpers;

using Core.Enums;
using Core.Extensions;
using Microsoft.AspNetCore.Razor.TagHelpers;

public class GradeTagHelper : TagHelper
{
    public Grade? Grade { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Grade is null)
            return;

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetContent(Grade.Name);
    }
}