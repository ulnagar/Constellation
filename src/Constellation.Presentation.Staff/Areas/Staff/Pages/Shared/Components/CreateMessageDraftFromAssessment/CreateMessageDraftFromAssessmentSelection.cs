namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.CreateMessageDraftFromAssessment;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

public sealed class CreateMessageDraftFromAssessmentSelection
{
    public bool IncludeStudents { get; set; }
    public bool IncludeParents { get; set; }
    public bool IncludeSchoolContacts { get; set; }
    public bool IncludeClassroomTeachers { get; set; }
}
