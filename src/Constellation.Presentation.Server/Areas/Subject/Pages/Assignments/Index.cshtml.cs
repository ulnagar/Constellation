using Constellation.Application.Common.CQRS.Subject.Assignments.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Subject.Pages.Assignments
{
    public class IndexModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public IndexModel(IUnitOfWork unitOfWork, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public ICollection<AssignmentForList> Assignments { get; set; }

        public async Task OnGet()
        {
            await GetClasses(_unitOfWork);

            Assignments = await _mediator.Send(new GetAssignmentsQuery());
        }
    }
}
