using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Admin.HangfireDashboard.Commands
{
    public class ToggleJobActivationCommand : IRequest
    {
        public Guid Id { get; set; }
    }

    public class ToggleJobActivationCommandHandler : IRequestHandler<ToggleJobActivationCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ToggleJobActivationCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(ToggleJobActivationCommand request, CancellationToken cancellationToken)
        {
            var record = await _unitOfWork.JobActivations.GetFromId(request.Id);

            if (record == null) return Unit.Value;

            record.IsActive = !record.IsActive;

            await _unitOfWork.CompleteAsync();

            return Unit.Value;
        }
    }
}
