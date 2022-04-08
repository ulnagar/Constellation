using MediatR;
using System;

namespace Constellation.Application.Features.Admin.HangfireDashboard.Commands
{
    public class ToggleJobActivationCommand : IRequest
    {
        public Guid Id { get; set; }
    }
}
