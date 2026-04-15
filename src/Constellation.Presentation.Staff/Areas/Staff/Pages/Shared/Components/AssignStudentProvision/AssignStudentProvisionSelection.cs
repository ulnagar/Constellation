namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AssignStudentProvision;

using Constellation.Core.Models.Assets.Enums;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Core.Models.Assessments.Identifiers;
using Core.Models.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Students.Identifiers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class AssignStudentProvisionSelection
{
    public StudentId StudentId { get; set; } = StudentId.Empty;
    public ProvisionId ProvisionId { get; set; } = ProvisionId.Empty;

    public required SelectList StudentList { get; set; }
    public required SelectList ProvisionList { get; set; }
}
