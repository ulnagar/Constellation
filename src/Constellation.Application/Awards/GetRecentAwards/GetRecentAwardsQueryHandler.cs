﻿namespace Constellation.Application.Awards.GetRecentAwards;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Awards.Models;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetRecentAwardsQueryHandler
    : IQueryHandler<GetRecentAwardsQuery, List<AwardResponse>>
{
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IStoredFileRepository _fileRepository;
    private readonly ILogger _logger;

    public GetRecentAwardsQueryHandler(
        IStudentAwardRepository awardRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IStoredFileRepository fileRepository,
        Serilog.ILogger logger)
    {
        _awardRepository = awardRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _fileRepository = fileRepository;
        _logger = logger.ForContext<GetRecentAwardsQuery>();
    }

    public async Task<Result<List<AwardResponse>>> Handle(GetRecentAwardsQuery request, CancellationToken cancellationToken)
    {
        List<AwardResponse> result = new();
        
        var awards = await _awardRepository.GetToRecentCount(request.Count, cancellationToken);

        var students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        var staff = await _staffRepository.GetAll(cancellationToken);

        foreach (var award in awards)
        {
            var student = students.FirstOrDefault(student => student.StudentId == award.StudentId);

            if (student is null)
            {
                _logger.Warning("Could not identify the student for award {@award}", award);
                continue;
            }

            var teacher = staff.FirstOrDefault(teacher => teacher.StaffId == award.TeacherId);

            var studentName = Name.Create(student.FirstName, string.Empty, student.LastName);
            var teacherName = Name.Create(teacher.FirstName, string.Empty, teacher.LastName);

            if (studentName.IsFailure || teacherName.IsFailure)
            {
                if (studentName.IsFailure)
                {
                    _logger.Warning("Could not create Name object from student {student} with error {@error}", student.DisplayName, studentName.Error);
                }

                if (teacherName.IsFailure)
                {
                    _logger.Warning("Could not create Name object from teacher {teacher} with error {@error}", teacher.DisplayName, teacherName.Error);
                }

                continue;
            }

            var hasCertificate = await _fileRepository.DoesAwardCertificateExistInDatabase(award.Id.ToString(), cancellationToken);

            var entry = new AwardResponse(
                award.Id,
                studentName.Value,
                student.CurrentGrade,
                student.School.Name,
                teacherName.Value,
                award.AwardedOn,
                award.Type,
                hasCertificate);

            result.Add(entry);
        }
        
        return result;
    }
}