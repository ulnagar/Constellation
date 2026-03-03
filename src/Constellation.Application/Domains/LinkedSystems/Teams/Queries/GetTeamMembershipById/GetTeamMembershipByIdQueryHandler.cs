namespace Constellation.Application.Domains.LinkedSystems.Teams.Queries.GetTeamMembershipById;

using Abstractions.Messaging;
using AppSettings.Models;
using Constellation.Application.Extensions;
using Constellation.Core.Models.AppSettings.Enums;
using Constellation.Core.Models.Covers.Repositories;
using Constellation.Core.Models.Faculties.ValueObjects;
using Constellation.Core.Models.LinkedSystems;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Repositories;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Errors;
using Core.Extensions;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.GroupTutorials;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTeamMembershipByIdQueryHandler
    : IQueryHandler<GetTeamMembershipByIdQuery, List<TeamMembershipResponse>>
{
    private readonly ITeamRepository _teamRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ICoverRepository _coverRepository;
    private readonly IAppSettingsService _appSettings;
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetTeamMembershipByIdQueryHandler(
        ITeamRepository teamRepository,
        IOfferingRepository offeringRepository,
        IFacultyRepository facultyRepository,
        IStudentRepository studentRepository,
        IEnrolmentRepository enrolmentRepository,
        IStaffRepository staffRepository,
        ITutorialRepository tutorialRepository,
        IGroupTutorialRepository groupTutorialRepository,
        ICourseRepository courseRepository,
        IDateTimeProvider dateTime,
        ILogger logger,
        ICoverRepository coverRepository,
        IAppSettingsService appSettings)
    {
        _teamRepository = teamRepository;
        _offeringRepository = offeringRepository;
        _facultyRepository = facultyRepository;
        _studentRepository = studentRepository;
        _enrolmentRepository = enrolmentRepository;
        _staffRepository = staffRepository;
        _tutorialRepository = tutorialRepository;
        _coverRepository = coverRepository;
        _appSettings = appSettings;
        _groupTutorialRepository = groupTutorialRepository;
        _courseRepository = courseRepository;
        _dateTime = dateTime;
        _logger = logger.ForContext<GetTeamMembershipByIdQuery>();
    }

    public async Task<Result<List<TeamMembershipResponse>>> Handle(GetTeamMembershipByIdQuery request, CancellationToken cancellationToken)
    {
        List<TeamMembershipResponse> returnData = new();

        Team? team = await _teamRepository.GetById(request.Id, cancellationToken);

        if (team is null)
        {
            _logger.Warning("Error: Task failed with error {@error}", DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);

            return Result.Failure<List<TeamMembershipResponse>>(DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);
        }

        TeamsConfiguration? teamsConfiguration = await _appSettings.Teams(cancellationToken);

        if (teamsConfiguration is null)
        {
            _logger
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(TeamsConfiguration)));

            return Result.Failure<List<TeamMembershipResponse>>(ApplicationErrors.InvalidConfiguration(nameof(TeamsConfiguration)));
        }

        CoversConfiguration? coversConfiguration = await _appSettings.Covers(cancellationToken);

        if (team.Description.Split(';').Contains("STUDENT"))
        {
            List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

            foreach (var student in students)
            {
                if (student.EmailAddress == EmailAddress.None)
                    continue;

                List<TeamMembershipResponse.TeamMembershipChannelResponse> studentChannels = (student.CurrentEnrolment is not null)
                    ? new() { new ($"{_dateTime.CurrentYear} - {student.CurrentEnrolment.Grade.AsName()}", TeamsMembershipLevel.Member.Value) }
                    : new();

                TeamMembershipResponse entry = new(
                    team.Id,
                    student.EmailAddress.Email,
                    TeamsMembershipLevel.Member.Value,
                    studentChannels);

                if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                    returnData.Add(entry);
            }

            List<StaffMember> staff = await _staffRepository.GetAllActive(cancellationToken);

            foreach (StaffMember staffMember in staff)
            {
                List<TeamMembershipResponse.TeamMembershipChannelResponse> staffChannels = new();

                List<Grade> grades = Enum.GetValues<Grade>().ToList();
                grades.Remove(Grade.SpecialProgram);

                bool channelOwnerExists = teamsConfiguration.StudentChannelOwners.Any(entry => entry.Key.Id == staffMember.Id);
                
                if (channelOwnerExists)
                {
                    List<Grade> ownerChannelGrades = teamsConfiguration.StudentChannelOwners.First(entry => entry.Key.Id == staffMember.Id).Value;

                    foreach (Grade grade in grades)
                    {
                        if (ownerChannelGrades!.Contains(grade))
                            staffChannels.Add(new($"{_dateTime.CurrentYear} - {grade.AsName()}", TeamsMembershipLevel.Owner.Value));
                        else
                            staffChannels.Add(new($"{_dateTime.CurrentYear} - {grade.AsName()}", TeamsMembershipLevel.Member.Value));
                    }
                }
                else
                {
                    foreach (Grade grade in grades)
                        staffChannels.Add(new($"{_dateTime.CurrentYear} - {grade.AsName()}", TeamsMembershipLevel.Member.Value));
                }

                bool teamOwnerExists = teamsConfiguration.StudentTeamOwners.Any(entry => entry.Key.Id == staffMember.Id);

                if (teamOwnerExists)
                {
                    TeamMembershipResponse entry = new(
                        team.Id,
                        staffMember.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value,
                        staffChannels);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);
                }
                else
                {
                    TeamMembershipResponse entry = new(
                        team.Id,
                        staffMember.EmailAddress.Email,
                        TeamsMembershipLevel.Member.Value,
                        staffChannels);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);
                }
            }

            foreach (EmailAddress owner in TeamsConfiguration.FallbackMandatoryOwners)
            {
                List<TeamMembershipResponse.TeamMembershipChannelResponse> masterChannels = new();

                List<Grade> masterGrades = Enum.GetValues<Grade>().ToList();
                masterGrades.Remove(Grade.SpecialProgram);

                foreach (Grade grade in masterGrades)
                    masterChannels.Add(new($"{_dateTime.CurrentYear} - {grade.AsName()}", TeamsMembershipLevel.Owner.Value));

                TeamMembershipResponse mandatoryOwnerEntry = new(
                    team.Id,
                    owner,
                    TeamsMembershipLevel.Owner.Value,
                    masterChannels);

                if (returnData.All(value => value.EmailAddress != mandatoryOwnerEntry.EmailAddress))
                    returnData.Add(mandatoryOwnerEntry);
            }

            return returnData;
        }

        if (team.Description.Split(';').Contains("CLASS"))
        {
            // Class Team which should have an offering
            List<Offering> offerings = await _offeringRepository.GetWithLinkedTeamResource(team.Name, cancellationToken);

            if (offerings.Count == 0)
            {
                //error
                _logger.Warning("Could not identify any Offering with active Resource for Team {id}", team.Id);

                return Result.Failure<List<TeamMembershipResponse>>(ResourceErrors.NoneOfTypeFound(ResourceType.MicrosoftTeam));
            }

            foreach (Offering offering in offerings)
            {
                // Enrolled Students
                List<Student> students = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

                foreach (Student student in students)
                {
                    if (student.EmailAddress == EmailAddress.None)
                        continue;

                    TeamMembershipResponse entry = new(
                        team.Id,
                        student.EmailAddress.Email,
                        TeamsMembershipLevel.Member.Value);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);
                }

                // Class Teachers
                List<StaffMember> teachers = await _staffRepository.GetCurrentTeachersForOffering(offering.Id, cancellationToken);

                foreach (StaffMember teacher in teachers)
                {
                    TeamMembershipResponse entry = new(
                        team.Id,
                        teacher.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);
                }

                // Covering Teachers
                List<string> coveringTeachers = await _coverRepository.GetCurrentTeacherEmailsForAccessProvisioning(offering.Id, cancellationToken);

                foreach (string teacher in coveringTeachers)
                {
                    TeamMembershipResponse entry = new(
                        team.Id,
                        teacher,
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);
                }

                // Head Teachers
                Faculty? faculty = await _facultyRepository.GetByOfferingId(offering.Id, cancellationToken);

                if (faculty is null)
                {
                    //error
                    _logger.Warning("Could not identify Faculty from offering {offering}.", offering.Name);

                    return Result.Failure<List<TeamMembershipResponse>>(new Error("Partners.Faculty.SearchFailed", $"Could not identify faculty from offering {offering.Name}."));
                }

                List<StaffMember> headTeachers = await _staffRepository.GetFacultyHeadTeachers(faculty.Id, cancellationToken);

                foreach (StaffMember teacher in headTeachers)
                {
                    TeamMembershipResponse entry = new(
                        team.Id,
                        teacher.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);
                }
                
                Course? course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

                if (course is null) 
                    continue;

                // Cover administrators
                // Only if class is being covered
                if (coveringTeachers.Count > 0)
                {
                    if (coversConfiguration is null)
                        continue;

                    foreach (var teacher in coversConfiguration.Contacts)
                    {
                        if (!teacher.Value.Contains(course.Grade))
                            continue;

                        TeamMembershipResponse teacherEntry = new(
                            team.Id,
                            teacher.Key.EmailAddress,
                            TeamsMembershipLevel.Owner.Value);

                        if (returnData.All(value => value.EmailAddress != teacherEntry.EmailAddress))
                            returnData.Add(teacherEntry);
                    }
                }

                // Deputy Principals
                ContactsConfiguration? deputyPrincipals = await _appSettings.Contacts(ContactPosition.DeputyPrincipal, cancellationToken);

                if (deputyPrincipals is null)
                    continue;

                foreach (var deputyPrincipal in deputyPrincipals.Contacts)
                {
                    if (!deputyPrincipal.Value.Contains(course.Grade))
                        continue;

                    TeamMembershipResponse deputyEntry = new(
                        team.Id,
                        deputyPrincipal.Key.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != deputyEntry.EmailAddress))
                        returnData.Add(deputyEntry);
                }

                // Learning and Support Teachers
                ContactsConfiguration? learningSupport = await _appSettings.Contacts(ContactPosition.LearningSupport, cancellationToken);

                if (learningSupport is null)
                    continue;

                foreach (var learningSupportTeacher in learningSupport.Contacts)
                {
                    if (!learningSupportTeacher.Value.Contains(course.Grade))
                        continue;

                    TeamMembershipResponse lastEntry = new(
                        team.Id,
                        learningSupportTeacher.Key.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != lastEntry.EmailAddress))
                        returnData.Add(lastEntry);
                }
            }
        }

        if (team.Description.Split(';').Contains("TUT"))
        {
            List<Tutorial> tutorials = await _tutorialRepository.GetWithLinkedTeam(team.Id, cancellationToken);

            if (tutorials.Count == 0)
            {
                //error
                _logger.Warning("Could not identify any Tutorial with active Resource for Team {id}", team.Id);

                return Result.Failure<List<TeamMembershipResponse>>(new Error("Tutorial.Team.NoMatch", "Could not find a matching Team for the Tutorial"));
            }

            foreach (Tutorial tutorial in tutorials)
            {
                // Enrolled Students
                List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByTutorialId(tutorial.Id, cancellationToken);

                List<StudentId> studentIds = enrolments
                    .Select(enrolment => enrolment.StudentId)
                    .Distinct()
                    .ToList();

                List<Student> matchingStudents = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

                // Student other offerings
                List<Offering> offerings = [];

                foreach (Student student in matchingStudents)
                {
                    if (student.EmailAddress == EmailAddress.None)
                        continue;

                    TeamMembershipResponse entry = new(
                        team.Id,
                        student.EmailAddress.Email,
                        TeamsMembershipLevel.Member.Value);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);

                    List<Offering> studentOfferings = await _offeringRepository.GetByStudentId(student.Id, cancellationToken);

                    foreach (var offering in studentOfferings)
                    {
                        if (offerings.Contains(offering))
                            continue;

                        offerings.Add(offering);
                    }
                }

                // Tutorial Teachers
                List<StaffId> tutorialTeacherIds = tutorial.Sessions
                    .Where(session => !session.IsDeleted)
                    .Select(session => session.StaffId)
                    .Distinct()
                    .ToList();

                List<StaffMember> teachers = await _staffRepository.GetListFromIds(tutorialTeacherIds, cancellationToken);

                foreach (StaffMember teacher in teachers)
                {
                    TeamMembershipResponse entry = new(
                        team.Id,
                        teacher.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);
                }

                // Class Teachers
                List<StaffId> staffIds = offerings
                .SelectMany(offering => offering.Teachers)
                    .Where(teacher =>
                        !teacher.IsDeleted &&
                        teacher.Type == AssignmentType.ClassroomTeacher)
                    .Select(entry => entry.StaffId)
                    .Distinct()
                    .ToList();

                List<StaffMember> offeringTeachers = await _staffRepository.GetListFromIds(staffIds, cancellationToken);

                foreach (StaffMember teacher in offeringTeachers)
                {
                    TeamMembershipResponse entry = new(
                        team.Id,
                        teacher.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);
                }

                // Head Teachers
                List<CourseId> courseIds = offerings
                    .Select(offering => offering.CourseId)
                    .Distinct()
                    .ToList();

                foreach (CourseId courseId in courseIds)
                {
                    Faculty? faculty = await _facultyRepository.GetByCourseId(courseId, cancellationToken);

                    if (faculty is null)
                    {
                        //error
                        _logger.Warning("Could not identify Faculty from Course Id {courseId}.", courseId);

                        continue;
                    }

                    List<StaffId> headTeacherIds = faculty.Members
                        .Where(facultyMember =>
                            !facultyMember.IsDeleted &&
                            facultyMember.Role == FacultyMembershipRole.Manager)
                        .Select(facultyMember => facultyMember.StaffId)
                        .Distinct()
                        .ToList();

                    List<StaffMember> headTeachers =
                        await _staffRepository.GetListFromIds(headTeacherIds, cancellationToken);

                    foreach (StaffMember teacher in headTeachers)
                    {
                        TeamMembershipResponse entry = new(
                            team.Id,
                            teacher.EmailAddress.Email,
                            TeamsMembershipLevel.Owner.Value);

                        if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                            returnData.Add(entry);
                    }
                }

                Grade? grade = tutorial.Name.GetGrade();

                if (grade is null)
                    continue;

                // Deputy Principals
                ContactsConfiguration? deputyPrincipals = await _appSettings.Contacts(ContactPosition.DeputyPrincipal, cancellationToken);

                if (deputyPrincipals is null)
                    continue;

                foreach (var deputyPrincipal in deputyPrincipals.Contacts)
                {
                    if (!deputyPrincipal.Value.Contains(grade.Value))
                        continue;

                    TeamMembershipResponse deputyEntry = new(
                        team.Id,
                        deputyPrincipal.Key.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != deputyEntry.EmailAddress))
                        returnData.Add(deputyEntry);
                }

                // Learning and Support Teachers
                ContactsConfiguration? learningSupport = await _appSettings.Contacts(ContactPosition.LearningSupport, cancellationToken);

                if (learningSupport is null)
                    continue;

                foreach (var learningSupportTeacher in learningSupport.Contacts)
                {
                    if (!learningSupportTeacher.Value.Contains(grade.Value))
                        continue;

                    TeamMembershipResponse lastEntry = new(
                        team.Id,
                        learningSupportTeacher.Key.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != lastEntry.EmailAddress))
                        returnData.Add(lastEntry);
                }
            }
        }

        if (team.Description.Split(';').Contains("GTUT"))
        {
            // Group Tutorial Team which will have a group tutorial
            string teamCourse = team.Name.Split(" - ")[2];

            GroupTutorial? tutorial = await _groupTutorialRepository.GetByName(teamCourse, cancellationToken);

            if (tutorial is null)
            {
                _logger.Warning("Could not identify Group Tutorial from name {name}", teamCourse);

                return Result.Failure<List<TeamMembershipResponse>>(new Error("GroupTutorials.GroupTutorial.NotFound", $"Could not identify Group Tutorial from name {teamCourse}."));
            }

            // Enrolled Students

            List<StudentId> studentIds = tutorial
                .CurrentEnrolments
                .Select(enrol => enrol.StudentId)
                .ToList();

            List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

            foreach (Student student in students)
            {
                TeamMembershipResponse entry = new(
                    team.Id,
                    student.EmailAddress.Email,
                    TeamsMembershipLevel.Member.Value);

                if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                    returnData.Add(entry);
            }

            // Class Teachers
            List<StaffId> teacherIds = tutorial
                .Teachers
                .Where(member =>
                    !member.IsDeleted &&
                    member.EffectiveFrom <= DateOnly.FromDateTime(DateTime.Today) &&
                    (!member.EffectiveTo.HasValue || member.EffectiveTo.Value >= DateOnly.FromDateTime(DateTime.Today)))
                .Select(member => member.StaffId)
                .ToList();

            List<StaffMember> teachers = await _staffRepository.GetListFromIds(teacherIds, cancellationToken);

            foreach (StaffMember teacher in teachers)
            {
                TeamMembershipResponse entry = new(
                    team.Id,
                    teacher.EmailAddress.Email,
                    TeamsMembershipLevel.Owner.Value);

                if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                    returnData.Add(entry);
            }
        }

        // Mandatory Owners
        if (teamsConfiguration.MandatoryOwners.Count > 0)
        {
            string[] tokens = team.Description.Split(';');

            Grade grade = Grade.SpecialProgram;

            foreach (var token in tokens)
            {
                if (grade == Grade.SpecialProgram)
                {
                    if (token.Contains("Year", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var gradeNum = token.Split(' ')[1];
                        bool success = Enum.TryParse(gradeNum, true, out grade);
                        if (!success || !Enum.IsDefined(grade))
                            grade = Grade.SpecialProgram;
                    }
                    else
                        grade = Grade.SpecialProgram;
                }
            }

            foreach (var mandatoryOwner in teamsConfiguration.MandatoryOwners)
            {
                if (grade != Grade.SpecialProgram && !mandatoryOwner.Value.Contains(grade))
                    continue;

                TeamMembershipResponse mandatoryOwnerEntry = new(
                    team.Id,
                    mandatoryOwner.Key.EmailAddress.Email,
                    TeamsMembershipLevel.Owner.Value);

                if (returnData.All(value => value.EmailAddress != mandatoryOwnerEntry.EmailAddress))
                    returnData.Add(mandatoryOwnerEntry);
            }
        }

        foreach (EmailAddress owner in TeamsConfiguration.FallbackMandatoryOwners)
        {
            TeamMembershipResponse mandatoryOwnerEntry = new(
                team.Id,
                owner,
                TeamsMembershipLevel.Owner.Value);

            if (returnData.All(value => value.EmailAddress != mandatoryOwnerEntry.EmailAddress))
                returnData.Add(mandatoryOwnerEntry);
        }

        return returnData;
    }
}
