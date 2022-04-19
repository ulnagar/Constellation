using Constellation.Core.Models;
using MediatR;
using System;

namespace Constellation.Application.Features.Attendance.Queries
{
    public class GetStudentAttendanceReportQuery : IRequest<StoredFile>
    {
        public string StudentId { get; set; }
        public DateTime StartDate { get; set; }
    }
}
