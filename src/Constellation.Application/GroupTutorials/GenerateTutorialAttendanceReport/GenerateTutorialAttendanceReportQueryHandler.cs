﻿namespace Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Core.Extensions;
using Helpers;
using System.Collections.Generic;
using System.IO;
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
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
            return Result.Failure<FileDto>(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));

        List<TutorialRoll> reportableRolls = tutorial.Rolls
            .Where(roll => roll.Status == Core.Enums.TutorialRollStatus.Submitted)
            .ToList();

        List<Student> studentEntities = await _studentRepository.GetListFromIds(tutorial.Enrolments.Select(enrolment => enrolment.StudentId).ToList(), cancellationToken);

        List<TutorialRollDetailsDto> rolls = new();

        foreach (TutorialRoll roll in reportableRolls )
        {
            List<TutorialRollStudentDetailsDto> students = new();

            foreach (TutorialRollStudent student in roll.Students)
            {
                Student entity = studentEntities.FirstOrDefault(entry => entry.StudentId == student.StudentId);

                if (entity is null)
                    continue;

                students.Add(new(
                    student.StudentId,
                    entity.DisplayName,
                    entity.CurrentGrade.AsName(),
                    student.Enrolled,
                    student.Present));
            }

            Staff staffMember = await _staffRepository.GetForExistCheck(roll.StaffId);

            rolls.Add(new(
                roll.Id,
                roll.SessionDate,
                roll.StaffId,
                staffMember.DisplayName,
                students));
        }

        TutorialDetailsDto dto = new(
            tutorial.Id,
            tutorial.Name,
            tutorial.StartDate,
            tutorial.EndDate,
            rolls);

        MemoryStream fileStream = await _excelService.CreateGroupTutorialAttendanceFile(dto);

        FileDto response = new()
        {
            FileData = fileStream.ToArray(),
            FileType = FileContentTypes.ExcelModernFile,
            FileName = "Tutorial Report.xlsx"
        };

        return response;
    }
}
