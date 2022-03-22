using Constellation.Application.Common.CQRS.Subject.Assignments.Queries;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Subject.Models.Assignments
{
    public class IndexViewModel : BaseViewModel
    {
        public ICollection<AssignmentForList> Assignments { get; set; } = new List<AssignmentForList>();
    }
}
