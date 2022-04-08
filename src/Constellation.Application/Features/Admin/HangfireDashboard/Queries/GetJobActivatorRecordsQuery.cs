using Constellation.Application.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Admin.HangfireDashboard.Queries
{
    public class GetJobActivatorRecordsQuery : IRequest<ICollection<JobActivation>>
    {
    }
}
