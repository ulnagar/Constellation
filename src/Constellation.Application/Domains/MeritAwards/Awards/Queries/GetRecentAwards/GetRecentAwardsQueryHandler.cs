namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetRecentAwards;

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
    private readonly IAttachmentRepository _fileRepository;
    private readonly ILogger _logger;

    public GetRecentAwardsQueryHandler(
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
        _logger = logger.ForContext<GetRecentAwardsQuery>();
    }

    public async Task<Result<List<AwardResponse>>> Handle(GetRecentAwardsQuery request, CancellationToken cancellationToken)
    {
        List<AwardResponse> result = new();
        
        List<StudentAward> awards = await _awardRepository.GetToRecentCount(request.Count, cancellationToken);

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        List<Staff> staff = await _staffRepository.GetAll(cancellationToken);

        foreach (StudentAward award in awards)
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
                _logger.Warning("Could not retrieve School Enrolment for student");
                continue;
            }

            Staff teacher = staff.FirstOrDefault(teacher => teacher.StaffId == award.TeacherId);
            Name teacherName = null;

            if (teacher is not null)
            {
                Result<Name> teacherNameRequest = Name.Create(teacher?.FirstName, string.Empty, teacher?.LastName);

                if (teacherNameRequest.IsFailure)
                    _logger.Warning("Could not create Name object from teacher {teacher} with error {@error}", teacher.DisplayName, teacherNameRequest.Error);
                else
                    teacherName = teacherNameRequest.Value;
            }

            bool hasCertificate = await _fileRepository.DoesAwardCertificateExistInDatabase(award.Id.ToString(), cancellationToken);

            AwardResponse entry = new AwardResponse(
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
