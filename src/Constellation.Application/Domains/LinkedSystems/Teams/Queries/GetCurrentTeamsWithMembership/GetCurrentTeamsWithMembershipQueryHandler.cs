namespace Constellation.Application.Domains.LinkedSystems.Teams.Queries.GetCurrentTeamsWithMembership;

using Abstractions.Messaging;
using AppSettings.Models;
using Constellation.Application.Extensions;
using Constellation.Core.Models.Covers.Repositories;
using Constellation.Core.Models.LinkedSystems;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Errors;
using Core.Extensions;
using Core.Models.AppSettings.Enums;
using Core.Models.Casuals;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.Faculties.ValueObjects;
using Core.Models.GroupTutorials;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Models.Subjects.Identifiers;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Configuration;
using Interfaces.Services;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentTeamsWithMembershipQueryHandler
: IQueryHandler<GetCurrentTeamsWithMembershipQuery, List<TeamWithMembership>>
{
    private readonly TeamsGatewayConfiguration _teamConfiguration;
    private readonly ITeamRepository _teamRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ICoverRepository _coverRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IAppSettingsService _appSettings;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetCurrentTeamsWithMembershipQueryHandler(
        IOptions<TeamsGatewayConfiguration> teamConfiguration,
        ITeamRepository teamRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IOfferingRepository offeringRepository,
        ITutorialRepository tutorialRepository,
        ICoverRepository coverRepository,
        IEnrolmentRepository enrolmentRepository,
        ICasualRepository casualRepository,
        IFacultyRepository facultyRepository,
        IGroupTutorialRepository groupTutorialRepository,
        IAppSettingsService appSettings,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _teamConfiguration = teamConfiguration.Value;
        _teamRepository = teamRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _offeringRepository = offeringRepository;
        _tutorialRepository = tutorialRepository;
        _coverRepository = coverRepository;
        _enrolmentRepository = enrolmentRepository;
        _casualRepository = casualRepository;
        _facultyRepository = facultyRepository;
        _groupTutorialRepository = groupTutorialRepository;
        _appSettings = appSettings;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<GetCurrentTeamsWithMembershipQuery>();
    }

    public async Task<Result<List<TeamWithMembership>>> Handle(GetCurrentTeamsWithMembershipQuery request,
        CancellationToken cancellationToken)
    {
        List<TeamWithMembership> teams = [];

        List<Team> serverTeams = await _teamRepository.GetAllCurrent(cancellationToken);

        serverTeams = serverTeams
            .Where(team =>
                team.Description.Split(';').Contains("CLASS") ||
                team.Description.Split(';').Contains("GTUT") ||
                team.Description.Split(';').Contains("TUTORIAL") ||
                team.Description.Split(';').Contains("STUDENTS"))
            .OrderBy(team => team.Name)
            .ToList();

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);
        List<StaffMember> staff = await _staffRepository.GetAllActive(cancellationToken);
        List<Offering> offerings = await _offeringRepository.GetAllActive(cancellationToken);
        List<Tutorial> tutorials = await _tutorialRepository.GetAllActive(cancellationToken);
        List<Cover> covers = await _coverRepository.GetAllCurrent(cancellationToken);
        List<Casual> casuals = await _casualRepository.GetAll(cancellationToken);
        CoversConfiguration? coversConfiguration = await _appSettings.Covers(cancellationToken);
        
        foreach (var team in serverTeams)
        {
            List<TeamWithMembership.Member> members = [];

            string[] tokens = team.Description.Split(';');

            Grade grade = Grade.SpecialProgram;

            foreach (var token in tokens)
            {
                if (grade == Grade.SpecialProgram)
                    Enum.TryParse(token, true, out grade);
            }
            
            // Student Teams
            if (tokens.Contains("STUDENT"))
            {
                Result<List<TeamWithMembership.Member>> studentTeamMembers = await ProcessStudentTeam(students, staff);

                if (studentTeamMembers.IsFailure)
                    return Result.Failure<List<TeamWithMembership>>(studentTeamMembers.Error);

                AddUnique(members, studentTeamMembers.Value);
            }

            // Class Teams
            if (tokens.Contains("CLASS"))
                AddUnique(members, await ProcessClassTeam(team, casuals, covers, offerings, students, staff, coversConfiguration, cancellationToken));

            // Tutorial Teams
            if (tokens.Contains("TUTORIAL"))
                AddUnique(members, await ProcessTutorialTeam(team, tutorials, students, staff, cancellationToken));

            // Group Tutorial Teams
            if (tokens.Contains("GTUT"))
                AddUnique(members, await ProcessGroupTutorialTeam(team, students, staff, cancellationToken));

            // Mandatory Owners
            Result<List<TeamWithMembership.Member>> mandatoryOwners = await ProcessMandatoryOwners(grade);

            if (mandatoryOwners.IsFailure)
                return Result.Failure<List<TeamWithMembership>>(mandatoryOwners.Error);

            AddUnique(members, mandatoryOwners.Value);

            teams.Add(new(
                team.Id,
                team.Name,
                team.Description,
                members));
        }

        return teams;
    }

    private static void AddUnique(
        List<TeamWithMembership.Member> list,
        List<TeamWithMembership.Member> newList)
    {
        foreach (var right in newList)
        {
            if (list.All(member => member.EmailAddress != right.EmailAddress))
            {
                list.Add(right);
                continue;
            }

            TeamWithMembership.Member? left = list.Find(member => member.EmailAddress == right.EmailAddress);

            if (left is null)
                continue;

            string newPermissionLevel = string.Empty;
            Dictionary<string, string> newChannels = new();

            if (left.PermissionLevel != TeamsMembershipLevel.Owner.Value && right.PermissionLevel == TeamsMembershipLevel.Owner.Value)
                newPermissionLevel = TeamsMembershipLevel.Owner.Value;

            foreach (KeyValuePair<string, string> leftChannel in left.Channels)
            {
                if (!right.Channels.TryGetValue(leftChannel.Key, out string? rightChannel))
                {
                    newChannels.Add(leftChannel.Key, leftChannel.Value);
                    continue;
                }

                if (leftChannel.Value != TeamsMembershipLevel.Owner.Value && rightChannel == TeamsMembershipLevel.Owner.Value)
                {
                    newChannels.Add(leftChannel.Key, TeamsMembershipLevel.Owner.Value);
                    continue;
                }

                newChannels.Add(leftChannel.Key, leftChannel.Value);
            }

            list.Remove(left);
            list.Add(new(
                left.EmailAddress,
                newPermissionLevel,
                newChannels));
        }
    }

    private async Task<List<TeamWithMembership.Member>> ProcessGroupTutorialTeam(
        Team team, 
        List<Student> students, 
        List<StaffMember> staff, 
        CancellationToken cancellationToken)
    {
        List<TeamWithMembership.Member> members = [];

        // Group Tutorial Team which will have a group tutorial
        string teamCourse = team.Name.Split(" - ")[2];

        GroupTutorial? tutorial = await _groupTutorialRepository.GetByName(teamCourse, cancellationToken);

        if (tutorial is null)
        {
            _logger.Warning("Could not identify Group Tutorial from name {name}", teamCourse);

            return members;
        }

        // Enrolled Students

        List<StudentId> studentIds = tutorial
            .CurrentEnrolments
            .Select(enrol => enrol.StudentId)
            .ToList();

        List<Student> enrolledStudents = students
            .Where(student => studentIds.Contains(student.Id))
            .ToList();

        foreach (Student student in enrolledStudents)
        {
            TeamWithMembership.Member entry = new(
                student.EmailAddress.Email,
                TeamsMembershipLevel.Member.Value,
                []);

            if (members.All(value => value.EmailAddress != entry.EmailAddress))
                members.Add(entry);
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

        List<StaffMember> teachers = staff
            .Where(staffMember => teacherIds.Contains(staffMember.Id))
            .ToList();

        foreach (StaffMember teacher in teachers)
        {
            TeamWithMembership.Member entry = new(
                teacher.EmailAddress.Email,
                TeamsMembershipLevel.Owner.Value,
                []);

            if (members.All(value => value.EmailAddress != entry.EmailAddress))
                members.Add(entry);
        }

        return members;
    }

    private async Task<List<TeamWithMembership.Member>> ProcessClassTeam(
        Team team,
        List<Casual> casuals,
        List<Cover> covers,
        List<Offering> offerings, 
        List<Student> students, 
        List<StaffMember> staff, 
        CoversConfiguration? configuration,
        CancellationToken cancellationToken = default)
    {
        List<TeamWithMembership.Member> members = [];

        // Class Team which should have an offering
        List<Offering> matchingOfferings = offerings
            .Where(offering => offering.Resources
                .Any(resource =>
                    resource.Type == ResourceType.MicrosoftTeam &&
                    ((MicrosoftTeamResource)resource).TeamName == team.Name))
            .ToList();

        if (matchingOfferings.Count == 0)
        {
            //error
            _logger.Warning("Could not identify any Offering with active Resource for Team {id}", team.Id);

            return members;
        }

        foreach (Offering offering in matchingOfferings)
        {
            // Enrolled Students
            List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByOfferingId(offering.Id, cancellationToken);

            List<Student> matchingStudents = students
                .Where(student =>
                    enrolments
                        .Select(enrolment => enrolment.StudentId)
                        .Contains(student.Id))
                .ToList();

            foreach (Student student in matchingStudents)
            {
                if (student.EmailAddress == EmailAddress.None)
                    continue;

                TeamWithMembership.Member entry = new(
                    student.EmailAddress.Email,
                    TeamsMembershipLevel.Member.Value,
                    []);

                if (members.All(value => value.EmailAddress != entry.EmailAddress))
                    members.Add(entry);
            }

            // Class Teachers
            List<StaffMember> teachers = staff.Where(staffMember => 
                    offering.Teachers.Any(assignment =>
                        assignment.Type == AssignmentType.ClassroomTeacher &&
                        assignment.StaffId == staffMember.Id))
                .ToList();

            foreach (StaffMember teacher in teachers)
            {
                TeamWithMembership.Member entry = new(
                    teacher.EmailAddress.Email,
                    TeamsMembershipLevel.Owner.Value,
                    []);

                if (members.All(value => value.EmailAddress != entry.EmailAddress))
                    members.Add(entry);
            }

            // Covering Teachers
            List<Cover> matchingCovers = covers.Where(cover => cover.OfferingId == offering.Id).ToList();

            if (matchingCovers.Count > 0)
            {
                foreach (Cover cover in matchingCovers)
                {
                    if (cover.TeacherType == CoverTeacherType.Staff)
                    {
                        var staffMember = staff.FirstOrDefault(staffMember => cover.TeacherId == staffMember.Id.ToString());

                        if (staffMember is not null)
                        {
                            TeamWithMembership.Member entry = new(
                                staffMember.EmailAddress.Email,
                                TeamsMembershipLevel.Owner.Value,
                                []);

                            if (members.All(value => value.EmailAddress != entry.EmailAddress))
                                members.Add(entry);
                        }
                    }

                    if (cover.TeacherType == CoverTeacherType.Casual)
                    {
                        var casual = casuals.FirstOrDefault(casual => cover.TeacherId == casual.Id.ToString());

                        if (casual is not null)
                        {
                            TeamWithMembership.Member entry = new(
                                casual.EmailAddress.Email,
                                TeamsMembershipLevel.Owner.Value,
                                []);

                            if (members.All(value => value.EmailAddress != entry.EmailAddress))
                                members.Add(entry);
                        }
                    }
                }

                // Cover administrators
                if (configuration is null)
                    continue;

                foreach (var teacher in configuration.Contacts)
                {
                    Grade? courseGrade = offering.Name.GetGrade();

                    if (courseGrade is null)
                        continue;

                    if (!teacher.Value.Contains(courseGrade.Value))
                        continue;

                    TeamWithMembership.Member teacherEntry = new(
                        teacher.Key.EmailAddress,
                        TeamsMembershipLevel.Owner.Value,
                        []);

                    if (members.All(value => value.EmailAddress != teacherEntry.EmailAddress))
                        members.Add(teacherEntry);
                }
            }

            // Head Teachers
            Faculty? faculty = await _facultyRepository.GetByCourseId(offering.CourseId, cancellationToken);

            if (faculty is null)
            {
                //error
                _logger.Warning("Could not identify Faculty from offering {offering}.", offering.Name);
            }
            else
            {
                List<StaffMember> headTeachers = staff.Where(staffMember =>
                        faculty.Members
                            .Where(facultyMember =>
                                !facultyMember.IsDeleted &&
                                facultyMember.Role == FacultyMembershipRole.Manager)
                            .Select(facultyMember => facultyMember.StaffId)
                            .Contains(staffMember.Id))
                    .ToList();

                foreach (StaffMember teacher in headTeachers)
                {
                    TeamWithMembership.Member entry = new(
                        teacher.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value,
                        []);

                    if (members.All(value => value.EmailAddress != entry.EmailAddress))
                        members.Add(entry);
                }
            }

            if (_teamConfiguration is not null)
            {
                Grade? grade = offering.Name.GetGrade();

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

                    TeamWithMembership.Member deputyEntry = new(
                        deputyPrincipal.Key.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value,
                        []);

                    if (members.All(value => value.EmailAddress != deputyEntry.EmailAddress))
                        members.Add(deputyEntry);
                }

                // Learning and Support Teachers
                ContactsConfiguration? learningSupport = await _appSettings.Contacts(ContactPosition.LearningSupport, cancellationToken);

                if (learningSupport is null)
                    continue;
                
                foreach (var learningSupportTeacher in learningSupport.Contacts)
                {
                    if (!learningSupportTeacher.Value.Contains(grade.Value))
                        continue;

                    TeamWithMembership.Member lastEntry = new(
                        learningSupportTeacher.Key.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value,
                        []);

                    if (members.All(value => value.EmailAddress != lastEntry.EmailAddress))
                        members.Add(lastEntry);
                }
            }
        }

        return members;
    }

    private async Task<List<TeamWithMembership.Member>> ProcessTutorialTeam(
    Team team,
    List<Tutorial> tutorials,
    List<Student> students,
    List<StaffMember> staff,
    CancellationToken cancellationToken = default)
    {
        List<TeamWithMembership.Member> members = [];

        // Tutorial Team which should have a tutorial
        List<Tutorial> matchingTutorials = tutorials
            .Where(offering => offering.Teams
                .Any(resource => resource.TeamId == team.Id))
            .ToList();

        if (matchingTutorials.Count == 0)
        {
            //error
            _logger.Warning("Could not identify any Tutorial with active Resource for Team {id}", team.Id);

            return members;
        }

        foreach (Tutorial tutorial in matchingTutorials)
        {
            // Enrolled Students
            List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByTutorialId(tutorial.Id, cancellationToken);

            List<Student> matchingStudents = students
                .Where(student =>
                    enrolments
                        .Select(enrolment => enrolment.StudentId)
                        .Contains(student.Id))
                .ToList();

            // Student other offerings
            List<Offering> offerings = [];

            foreach (Student student in matchingStudents)
            {
                if (student.EmailAddress == EmailAddress.None)
                    continue;

                TeamWithMembership.Member entry = new(
                    student.EmailAddress.Email,
                    TeamsMembershipLevel.Member.Value,
                    []);

                if (members.All(value => value.EmailAddress != entry.EmailAddress))
                    members.Add(entry);

                List<Offering> studentOfferings = await _offeringRepository.GetByStudentId(student.Id, cancellationToken);

                foreach (var offering in studentOfferings)
                {
                    if (offerings.Contains(offering))
                        continue;

                    offerings.Add(offering);
                }
            }

            // Tutorial Teachers
            List<StaffMember> teachers = staff.Where(staffMember =>
                    tutorial.Sessions.Any(session =>
                        !session.IsDeleted &&
                        session.StaffId == staffMember.Id))
                .ToList();

            foreach (StaffMember teacher in teachers)
            {
                TeamWithMembership.Member entry = new(
                    teacher.EmailAddress.Email,
                    TeamsMembershipLevel.Owner.Value,
                    []);

                if (members.All(value => value.EmailAddress != entry.EmailAddress))
                    members.Add(entry);
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

            List<StaffMember> offeringTeachers = staff.Where(staffMember =>
                    staffIds.Contains(staffMember.Id))
                .ToList();

            foreach (StaffMember teacher in offeringTeachers)
            {
                TeamWithMembership.Member entry = new(
                    teacher.EmailAddress.Email,
                    TeamsMembershipLevel.Owner.Value,
                    []);

                if (members.All(value => value.EmailAddress != entry.EmailAddress))
                    members.Add(entry);
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

                List<StaffMember> headTeachers = staff.Where(staffMember =>
                        faculty.Members
                            .Where(facultyMember =>
                                !facultyMember.IsDeleted &&
                                facultyMember.Role == FacultyMembershipRole.Manager)
                            .Select(facultyMember => facultyMember.StaffId)
                            .Contains(staffMember.Id))
                    .ToList();

                foreach (StaffMember teacher in headTeachers)
                {
                    TeamWithMembership.Member entry = new(
                        teacher.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value,
                        []);

                    if (members.All(value => value.EmailAddress != entry.EmailAddress))
                        members.Add(entry);
                }
            }
            
            if (_teamConfiguration is not null)
            {
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

                    TeamWithMembership.Member deputyEntry = new(
                        deputyPrincipal.Key.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value,
                        []);

                    if (members.All(value => value.EmailAddress != deputyEntry.EmailAddress))
                        members.Add(deputyEntry);
                }

                // Learning and Support Teachers
                ContactsConfiguration? learningSupport = await _appSettings.Contacts(ContactPosition.LearningSupport, cancellationToken);

                if (learningSupport is null)
                    continue;

                foreach (var learningSupportTeacher in learningSupport.Contacts)
                {
                    if (!learningSupportTeacher.Value.Contains(grade.Value))
                        continue;

                    TeamWithMembership.Member lastEntry = new(
                        learningSupportTeacher.Key.EmailAddress.Email,
                        TeamsMembershipLevel.Owner.Value,
                        []);

                    if (members.All(value => value.EmailAddress != lastEntry.EmailAddress))
                        members.Add(lastEntry);
                }
            }
        }

        return members;
    }

    private async Task<Result<List<TeamWithMembership.Member>>> ProcessMandatoryOwners(Grade grade)
    {
        List<TeamWithMembership.Member> members = [];

        TeamsConfiguration? configuration = await _appSettings.Teams();

        if (configuration is null)
        {
            _logger
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(TeamsConfiguration)));

            return Result.Failure<List<TeamWithMembership.Member>>(ApplicationErrors.InvalidConfiguration(nameof(TeamsConfiguration)));
        }

        if (configuration.MandatoryOwners.Count > 0)
        {
            foreach (var mandatoryOwner in configuration.MandatoryOwners)
            {
                if (grade != Grade.SpecialProgram && !mandatoryOwner.Value.Contains(grade))
                    continue;

                TeamWithMembership.Member mandatoryOwnerEntry = new(
                    mandatoryOwner.Key.EmailAddress.Email,
                    TeamsMembershipLevel.Owner.Value,
                    []);

                if (members.All(value => value.EmailAddress != mandatoryOwnerEntry.EmailAddress))
                    members.Add(mandatoryOwnerEntry);
            }
        }

        foreach (EmailAddress owner in TeamsConfiguration.FallbackMandatoryOwners)
        {
            TeamWithMembership.Member entry = new(
                owner,
                TeamsMembershipLevel.Owner.Value,
                []);

            if (members.All(value => value.EmailAddress != entry.EmailAddress))
                members.Add(entry);
        }
        
        return members;
    }

    private async Task<Result<List<TeamWithMembership.Member>>> ProcessStudentTeam(List<Student> students, List<StaffMember> staff)
    {
        List<TeamWithMembership.Member> members = [];

        TeamsConfiguration? configuration = await _appSettings.Teams();

        if (configuration is null)
        {
            _logger
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(TeamsConfiguration)));

            return Result.Failure<List<TeamWithMembership.Member>>(ApplicationErrors.InvalidConfiguration(nameof(TeamsConfiguration)));
        }
        
        foreach (var student in students)
        {
            if (student.EmailAddress == EmailAddress.None)
                continue;

            Dictionary<string, string> studentChannels = new();
            if (student.CurrentEnrolment is not null)
                studentChannels.Add($"{_dateTime.CurrentYear} - {student.CurrentEnrolment.Grade.AsName()}", TeamsMembershipLevel.Member.Value);
                    
            TeamWithMembership.Member entry = new(
                student.EmailAddress.Email,
                TeamsMembershipLevel.Member.Value,
                studentChannels);

            if (members.All(value => value.EmailAddress != entry.EmailAddress))
                members.Add(entry);
        }

        foreach (StaffMember staffMember in staff)
        {
            Dictionary<string, string> staffChannels = new();

            bool ownerExists = configuration.StudentChannelOwners.Any(entry => entry.Key.Id == staffMember.Id);

            if (ownerExists)
            {
                List<Grade> ownerGrades = configuration.StudentChannelOwners.First(entry => entry.Key.Id == staffMember.Id).Value;

                foreach (Grade grade in Enum.GetValues<Grade>())
                {
                    if (ownerGrades!.Contains(grade))
                        staffChannels.Add($"{_dateTime.CurrentYear} - {grade.AsName()}", TeamsMembershipLevel.Owner.Value);
                    else
                        staffChannels.Add($"{_dateTime.CurrentYear} - {grade.AsName()}", TeamsMembershipLevel.Member.Value);
                }
            }
            else
            {
                foreach (Grade grade in Enum.GetValues<Grade>())
                    staffChannels.Add($"{_dateTime.CurrentYear} - {grade.AsName()}", TeamsMembershipLevel.Member.Value);
            }

            if (configuration.StudentTeamOwners.ContainsKey(staffMember))
            {
                TeamWithMembership.Member entry = new(
                    staffMember.EmailAddress.Email,
                    TeamsMembershipLevel.Owner.Value,
                    staffChannels);

                if (members.All(value => value.EmailAddress != entry.EmailAddress))
                    members.Add(entry);
            }
            else
            {
                TeamWithMembership.Member entry = new(
                    staffMember.EmailAddress.Email,
                    TeamsMembershipLevel.Member.Value,
                    staffChannels);

                if (members.All(value => value.EmailAddress != entry.EmailAddress))
                    members.Add(entry);
            }
        }

        return members;
    }
}
