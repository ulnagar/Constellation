namespace Constellation.Application.Features.Portal.School.Timetables.Queries;

using Constellation.Application.DTOs;
using MediatR;

public sealed class GetStudentTimetableExportQuery : IRequest<FileDto>
{
    public StudentTimetableDataDto Data { get; set; }
}