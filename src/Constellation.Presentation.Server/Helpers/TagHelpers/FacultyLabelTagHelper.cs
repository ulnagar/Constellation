namespace Constellation.Presentation.Server.Helpers.TagHelpers;

using Application.Faculties.GetFacultiesSummary;
using Microsoft.AspNetCore.Razor.TagHelpers;

public class FacultyLabelTagHelper : TagHelper
{
    public FacultySummaryResponse Faculty { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "badge");
        output.Attributes.SetAttribute("style", $"background-color: {Faculty.Colour}; color: white");
        output.Content.SetContent(Faculty.Name);
    }
}
