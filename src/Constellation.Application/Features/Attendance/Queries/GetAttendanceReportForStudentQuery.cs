namespace Constellation.Application.Features.Attendance.Queries;

using MediatR;
using System;
using System.IO;

public class GetAttendanceReportForStudentQuery : IRequest<MemoryStream>
{
    public string StudentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

