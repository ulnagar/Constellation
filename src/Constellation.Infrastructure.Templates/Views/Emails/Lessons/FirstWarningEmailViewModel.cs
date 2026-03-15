namespace Constellation.Infrastructure.Templates.Views.Emails.Lessons;

using Constellation.Application.DTOs;
using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public sealed class FirstWarningEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Lessons/FirstWarningEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    public readonly string Link = $"{BaseUrl}";

    public required string SchoolName { get; set; }
    public List<LessonEmail.LessonItem> Lessons { get; set; } = [];
}
