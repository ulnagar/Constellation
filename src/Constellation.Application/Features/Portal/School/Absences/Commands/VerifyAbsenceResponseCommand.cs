using MediatR;
using System;

namespace Constellation.Application.Features.Portal.School.Absences.Commands
{
    public class VerifyAbsenceResponseCommand : IRequest
    {
        public Guid AbsenceId { get; set; }
        public Guid ResponseId {get;set; }
        public string UserEmail {get;set; }
        public string Comment { get; set; }
    }
}
