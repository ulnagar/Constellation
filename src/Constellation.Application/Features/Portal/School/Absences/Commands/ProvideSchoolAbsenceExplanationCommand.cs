using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.Features.Portal.School.Absences.Commands
{
    public class ProvideSchoolAbsenceExplanationCommand : IRequest
    {
        public Guid AbsenceId { get; set; }
        [Required]
        [MinLength(5, ErrorMessage = "You must provide a longer explanation for this absence.")]
        public string Comment { get; set; }
        public string UserEmail { get; set; }
    }
}
