namespace Constellation.Application.Domains.Messaging.Drafts.Commands.AddAssessmentRecipientsToDraft;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Assessments.Repositories;
using Constellation.Core.Models.Messaging.Drafts;
using Constellation.Core.Models.Messaging.Drafts.Repositories;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.SchoolContacts.Repositories;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Families;
using Core.Models.Identifiers;
using Core.Models.Offerings;
using Core.Models.Offerings.Enums;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using Serilog;

internal sealed class AddAssessmentRecipientsToDraftCommandHandler
    : ICommandHandler<AddAssessmentRecipientsToDraftCommand>
{
    private readonly IMessageDraftRepository _draftRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ILogger _logger;

    public AddAssessmentRecipientsToDraftCommandHandler(
        IMessageDraftRepository draftRepository,
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        IStaffRepository staffRepository,
        IOfferingRepository offeringRepository,
        IAssessmentRepository assessmentRepository,
        ILogger logger)
    {
        _draftRepository = draftRepository;
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _contactRepository = contactRepository;
        _staffRepository = staffRepository;
        _offeringRepository = offeringRepository;
        _assessmentRepository = assessmentRepository;
        _logger = logger
            .ForContext<AddAssessmentRecipientsToDraftCommand>();
    }

    public async Task<Result> Handle(AddAssessmentRecipientsToDraftCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(AddAssessmentRecipientsToDraftCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to prepare draft message linked to Assessment");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        List<MessageRecipient> recipients = [];

        List <StudentId> studentIds = request.IncludeStudents || request.IncludeParents
            ? assessment.Students.Select(entry => entry.StudentId).Distinct().ToList()
            : [];

        List<SchoolCode> schoolCodes = request.IncludeSchoolContacts
            ? assessment.Students.Select(entry => entry.SchoolCode).Distinct().ToList()
            : [];

        List<Offering> classes = request.IncludeClassroomTeachers
            ? await _offeringRepository.GetActiveByCourseId(assessment.CourseId, cancellationToken)
            : [];

        if (request.IncludeParents)
        {
            foreach (StudentId studentId in studentIds)
            {
                List<Family> families = await _familyRepository.GetFamiliesByStudentId(studentId, cancellationToken);

                List<Parent> parents = families.SelectMany(entry => entry.Parents).ToList();

                foreach (Parent parent in parents)
                {
                    if (parent.EmailAddress == EmailAddress.None)
                        continue;

                    if (recipients.Any(entry => entry.EmailAddress == parent.EmailAddress))
                        continue;

                    recipients.Add(new(parent.EmailAddress, parent.Name.DisplayName));
                }
            }
        }

        if (request.IncludeStudents)
        {
            List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

            foreach (Student student in students)
            {
                if (student.EmailAddress == EmailAddress.None)
                    continue;

                if (recipients.Any(entry => entry.EmailAddress == student.EmailAddress))
                    continue;

                recipients.Add(new(student.EmailAddress, student.Name.DisplayName));
            }
        }

        if (request.IncludeSchoolContacts)
        {
            foreach (SchoolCode schoolCode in schoolCodes)
            {
                List<SchoolContact> contacts = await _contactRepository.GetBySchoolAndRole(schoolCode, Position.Coordinator, cancellationToken);

                foreach (SchoolContact contact in contacts)
                {
                    if (contact.EmailAddress == EmailAddress.None)
                        continue;

                    if (recipients.Any(entry => entry.EmailAddress == contact.EmailAddress))
                        continue;

                    recipients.Add(new(contact.EmailAddress, contact.Name.DisplayName));
                }
            }
        }

        if (request.IncludeClassroomTeachers)
        {
            List<StaffId> staffIds = classes
                .SelectMany(entry => entry.Teachers)
                .Where(entry => entry.Type == AssignmentType.ClassroomTeacher)
                .Select(entry => entry.StaffId)
                .ToList();

            List<StaffMember> teachers = await _staffRepository.GetListFromIds(staffIds, cancellationToken);

            foreach (StaffMember teacher in teachers)
            {
                if (teacher.EmailAddress == EmailAddress.None)
                    continue;

                if (recipients.Any(entry => entry.EmailAddress == teacher.EmailAddress))
                    continue;

                recipients.Add(new(teacher.EmailAddress, teacher.Name.DisplayName));
            }
        }

        if (recipients.Count == 0)
            return Result.Success();

        await _draftRepository.DeleteDraft(request.UserId, cancellationToken);
        await _draftRepository.GetDraft(request.UserId, "Assessments", cancellationToken);

        foreach (var recipient in recipients)
            await _draftRepository.AddRecipient(recipient, request.UserId, cancellationToken);

        return Result.Success();
    }
}
