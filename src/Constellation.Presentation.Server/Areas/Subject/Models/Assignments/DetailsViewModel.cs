using Constellation.Application.Features.Subject.Assignments.Models;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Subject.Models.Assignments
{
    public class DetailsViewModel : BaseViewModel
    {
        // Assessment Overview
        public AssignmentForList Assignment { get; set; }

        // Submitted files
        public ICollection<AssignmentSubmissionForList> Submissions { get; set; }
    }
}
