using MediatR;
using System;

namespace Constellation.Application.Features.Admin.HangfireDashboard.Commands
{
    public class RunJobManuallyCommand : IRequest
    {
        public Guid Id { get; set; }
    }
}
