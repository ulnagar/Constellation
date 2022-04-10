using Constellation.Application.Features.Portal.School.Absences.Models;
using MediatR;
using System;

namespace Constellation.Application.Features.Portal.School.Absences.Queries
{
    public class GetAbsenceResponseForVerificationQuery : IRequest<PartialAbsenceResponseForVerification>
    {
        public Guid Id { get; set; }
    }
}
