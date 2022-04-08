using Constellation.Application.Features.Subject.Assignments.Models;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Subject.Models.Assignments
{
    public class IndexViewModel : BaseViewModel
    {
        public ICollection<AssignmentForList> Assignments { get; set; } = new List<AssignmentForList>();
    }
}
