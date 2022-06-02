using Constellation.Application.Features.Portal.School.Timetables.Models;
using Constellation.Core.Models;
using MediatR;

namespace Constellation.Application.Features.Portal.School.Timetables.Queries
{
    public class GetStudentTimetableExportQuery : IRequest<StoredFile>
    {
        public StudentTimetableDataDto Data { get; set; }
    }
}
