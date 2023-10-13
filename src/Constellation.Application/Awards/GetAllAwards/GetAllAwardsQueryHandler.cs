namespace Constellation.Application.Awards.GetAllAwards;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Awards.Models;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllAwardsQueryHandler
    : IQueryHandler<GetAllAwardsQuery, List<AwardResponse>>
{
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IAttachmentRepository _fileRepository;
    private readonly ILogger _logger;

    public GetAllAwardsQueryHandler(
        IStudentAwardRepository awardRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IAttachmentRepository fileRepository,
        Serilog.ILogger logger)
    {
        _awardRepository = awardRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _fileRepository = fileRepository;
        _logger = logger.ForContext<GetAllAwardsQuery>();
    }

    public async Task<Result<List<AwardResponse>>> Handle(GetAllAwardsQuery request, CancellationToken cancellationToken)
    {
        List<AwardResponse> result = new();

        var awards = request.CurrentYearOnly switch
        {
            true => await _awardRepository.GetFromYear(DateTime.Today.Year, cancellationToken),
            false => await _awardRepository.GetAll(cancellationToken)
        };

        var students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        var currentStudentAwards = awards
            .Where(award =>
                students
                    .Select(student => student.StudentId)
                    .Contains(award.StudentId))
            .ToList();

        var staff = await _staffRepository.GetAll(cancellationToken);

        foreach (var award in currentStudentAwards)
        {
            var student = students.FirstOrDefault(student => student.StudentId == award.StudentId);

            if (student is null)
            {
                _logger.Warning("Could not identify the student for award {@award}", award);
                continue;
            }

            Name studentName = null;

            if (student is not null)
            {
                var studentNameRequest = Name.Create(student.FirstName, string.Empty, student.LastName);

                if (studentNameRequest.IsSuccess)
                    studentName = studentNameRequest.Value;
                else
                    _logger.Warning("Could not create Name object from student {student} with error {@error}", student.DisplayName, studentNameRequest.Error);
            }

            var teacher = staff.FirstOrDefault(teacher => teacher.StaffId == award.TeacherId);

            Name teacherName = null;

            if (teacher is not null)
            {
                var teacherNameRequest = Name.Create(teacher.FirstName, string.Empty, teacher.LastName);

                if (teacherNameRequest.IsSuccess)
                    teacherName = teacherNameRequest.Value;
                else
                    _logger.Warning("Could not create Name object from teacher {teacher} with error {@error}", teacher.DisplayName, teacherNameRequest.Error);
            }

            if (studentName is null)
                continue;

            var hasCertificate = await _fileRepository.DoesAwardCertificateExistInDatabase(award.Id.ToString(), cancellationToken);

            var entry = new AwardResponse(
                award.Id,
                studentName,
                student.CurrentGrade,
                student.School.Name,
                teacherName,
                award.AwardedOn,
                award.Type,
                hasCertificate);

            result.Add(entry);
        }

        return result;
    }
}
