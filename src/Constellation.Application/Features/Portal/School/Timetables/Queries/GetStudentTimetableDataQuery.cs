using Constellation.Application.Features.Portal.School.Timetables.Models;
using MediatR;

namespace Constellation.Application.Features.Portal.School.Timetables.Queries
{
    public class GetStudentTimetableDataQuery : IRequest<StudentTimetableDataDto>
    {
        public string StudentId { get; set; }
    }
}
