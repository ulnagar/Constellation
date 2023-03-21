namespace Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GenerateTutorialAttendanceReportQueryHandler
    : IQueryHandler<GenerateTutorialAttendanceReportQuery, FileDto>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IExcelService _excelService;

    public GenerateTutorialAttendanceReportQueryHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IExcelService excelService,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _excelService = excelService;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<FileDto>> Handle(GenerateTutorialAttendanceReportQuery request, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository.GetWithRollsById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            return Result.Failure<FileDto>(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));
        }

        var reportableRolls = tutorial.Rolls.Where(roll => roll.Status == Core.Enums.TutorialRollStatus.Submitted).ToList();

        var studentLinks = reportableRolls.SelectMany(roll => roll.Students).Select(student => student.StudentId).Distinct().ToList();
        var studentEntities = await _studentRepository.GetListFromIds(studentLinks, cancellationToken);

        var rolls = new List<TutorialRollDetailsDto>();

        foreach (var roll in reportableRolls )
        {
            var students = new List<TutorialRollStudentDetailsDto>();

            foreach (var student in roll.Students)
            {
                var entity = studentEntities.FirstOrDefault(entry => entry.StudentId == student.StudentId);

                if (entity is null)
                    continue;

                students.Add(new(
                    student.StudentId,
                    entity.DisplayName,
                    entity.CurrentGrade.AsName(),
                    student.Enrolled,
                    student.Present));
            }

            var staffMember = await _staffRepository.GetForExistCheck(roll.StaffId);

            rolls.Add(new(
                roll.Id,
                roll.SessionDate,
                roll.StaffId,
                staffMember.DisplayName,
                students));
        }

        var dto = new TutorialDetailsDto(
            tutorial.Id,
            tutorial.Name,
            tutorial.StartDate,
            tutorial.EndDate,
            rolls);

        var fileStream = await _excelService.CreateGroupTutorialAttendanceFile(dto);

        var response = new FileDto
        {
            FileData = fileStream.ToArray(),
            FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = "Tutorial Report.xlsx"
        };

        return response;
    }
}
