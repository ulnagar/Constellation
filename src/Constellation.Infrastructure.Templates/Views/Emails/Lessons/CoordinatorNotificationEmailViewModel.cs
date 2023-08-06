namespace Constellation.Infrastructure.Templates.Views.Emails.Lessons;

using Constellation.Application.DTOs;
using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public class CoordinatorNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    public string SchoolName { get; set; }
    public List<LessonEmail.LessonItem> Lessons { get; set; } = new();
}
