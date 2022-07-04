using MediatR;
using System;

namespace Constellation.Application.Features.Awards.Commands
{
    public class CreateStudentAwardCommand : IRequest
    {
        public string StudentId { get; set; }
        public DateTime AwardedOn { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
    }
}
