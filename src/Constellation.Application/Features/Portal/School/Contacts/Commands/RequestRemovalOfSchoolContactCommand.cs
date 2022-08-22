using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.Features.Portal.School.Contacts.Commands
{
    public class RequestRemovalOfSchoolContactCommand : IRequest
    {
        public int AssignmentId { get; set; }
        [Required]
        public string Comment { get; set; }
        public string CancelledBy { get; set; }
        public DateTime CancelledAt { get; set; }
    }
}
