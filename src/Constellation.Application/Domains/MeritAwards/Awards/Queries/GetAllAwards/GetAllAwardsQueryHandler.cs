namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAllAwards;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Students.Repositories;
using Core.Models;
using Core.Models.Awards;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;
using Models;
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
        ILogger logger)
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

        List<StudentAward> awards = request.CurrentYearOnly switch
        {
            true => await _awardRepository.GetFromYear(DateTime.Today.Year, cancellationToken),
            false => await _awardRepository.GetAll(cancellationToken)
        };

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        List<StudentAward> currentStudentAwards = awards
            .Where(award =>
                students
                    .Select(student => student.Id)
                    .Contains(award.StudentId))
            .ToList();

        List<Staff> staff = await _staffRepository.GetAll(cancellationToken);

        foreach (StudentAward award in currentStudentAwards)
        {
            Student student = students.FirstOrDefault(student => student.Id == award.StudentId);

            if (student is null)
            {
                _logger.Warning("Could not identify the student for award {@award}", award);
                continue;
            }

            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null)
            {
                _logger.Warning("Could not retrieve current School Enrolment for student");
                continue;
            }
            
            Staff teacher = staff.FirstOrDefault(teacher => teacher.StaffId == award.TeacherId);

            Name teacherName = null;

            if (teacher is not null)
            {
                Result<Name> teacherNameRequest = Name.Create(teacher.FirstName, string.Empty, teacher.LastName);

                if (teacherNameRequest.IsSuccess)
                    teacherName = teacherNameRequest.Value;
                else
                    _logger.Warning("Could not create Name object from teacher {teacher} with error {@error}", teacher.DisplayName, teacherNameRequest.Error);
            }

            bool hasCertificate = await _fileRepository.DoesAwardCertificateExistInDatabase(award.Id.ToString(), cancellationToken);

            AwardResponse entry = new(
                award.Id,
                student.Name,
                enrolment.Grade,
                enrolment.SchoolName,
                teacherName,
                award.AwardedOn,
                award.Type,
                hasCertificate);

            result.Add(entry);
        }

        return result;
    }
}
