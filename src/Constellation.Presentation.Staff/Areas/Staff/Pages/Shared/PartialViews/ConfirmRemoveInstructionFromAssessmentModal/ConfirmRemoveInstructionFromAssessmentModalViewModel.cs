namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.ConfirmRemoveInstructionFromAssessmentModal;

using Core.Models.Assessments.Identifiers;
using System;
using System.Collections.Generic;
using System.Text;

public sealed class ConfirmRemoveInstructionFromAssessmentModalViewModel
{
    public required AssessmentInstructionId InstructionId { get; set; }
}
