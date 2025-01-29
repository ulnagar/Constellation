namespace Constellation.Application.Teams.GetTeamMembershipById;

using Application.Models.Identity;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Extensions;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.ValueObjects;
using Interfaces.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
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
    private readonly IStaffRepository _staffRepository;
    private readonly IClassCoverRepository _coverRepository;
    private readonly TeamsGatewayConfiguration _teamsConfiguration;
    private readonly AppConfiguration _configuration;
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public GetTeamMembershipByIdQueryHandler(
        ITeamRepository teamRepository,
        IOfferingRepository offeringRepository,
        IFacultyRepository facultyRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IGroupTutorialRepository groupTutorialRepository,
        ICourseRepository courseRepository,
        IDateTimeProvider dateTime,
        UserManager<AppUser> userManager,
        ILogger logger,
        IClassCoverRepository coverRepository,
        IOptions<AppConfiguration> configuration,
        IOptions<TeamsGatewayConfiguration> teamsConfiguration)
    {
        _teamRepository = teamRepository;
        _offeringRepository = offeringRepository;
        _facultyRepository = facultyRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _coverRepository = coverRepository;
        _teamsConfiguration = teamsConfiguration.Value;
        _configuration = configuration.Value;
        _groupTutorialRepository = groupTutorialRepository;
        _courseRepository = courseRepository;
        _dateTime = dateTime;
        _userManager = userManager;
        _logger = logger.ForContext<GetTeamMembershipByIdQuery>();
    }

    public async Task<Result<List<TeamMembershipResponse>>> Handle(GetTeamMembershipByIdQuery request, CancellationToken cancellationToken)
    {
        List<TeamMembershipResponse> returnData = new();

        Team team = await _teamRepository.GetById(request.Id, cancellationToken);

        if (team is null)
        {
            _logger.Warning("Error: Task failed with error {@error}", DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);

            return Result.Failure<List<TeamMembershipResponse>>(DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);
        }

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

            List<Staff> staff = await _staffRepository.GetAllActive(cancellationToken);

            foreach (Staff staffMember in staff)
            {
                List<TeamMembershipResponse.TeamMembershipChannelResponse> staffChannels = new();

                foreach (var grade in _teamsConfiguration.StudentTeamChannelOwnerIds)
                {
                    if (grade.Value.Contains(staffMember.StaffId))
                        staffChannels.Add(new ($"{_dateTime.CurrentYear} - {grade.Key.AsName()}", TeamsMembershipLevel.Owner.Value));
                    else
                        staffChannels.Add(new($"{_dateTime.CurrentYear} - {grade.Key.AsName()}", TeamsMembershipLevel.Member.Value));
                }

                if (_teamsConfiguration.StudentTeamOwnerIds.Contains(staffMember.StaffId))
                {
                    TeamMembershipResponse entry = new(
                        team.Id,
                        staffMember.EmailAddress,
                        TeamsMembershipLevel.Owner.Value,
                        staffChannels);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);
                }
                else
                {
                    TeamMembershipResponse entry = new(
                        team.Id,
                        staffMember.EmailAddress,
                        TeamsMembershipLevel.Member.Value,
                        staffChannels);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);
                }
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
                List<Staff> teachers = await _staffRepository.GetCurrentTeachersForOffering(offering.Id, cancellationToken);

                foreach (Staff teacher in teachers)
                {
                    TeamMembershipResponse entry = new(
                        team.Id,
                        teacher.EmailAddress,
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);
                }

                // Covering Teachers
                List<string> coveringTeachers = await _coverRepository.GetCurrentCoveringTeachersForOffering(offering.Id, cancellationToken);

                foreach (string teacher in coveringTeachers)
                {
                    TeamMembershipResponse entry = new(
                        team.Id,
                        teacher,
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);

                    // Cover administrators
                    IList<AppUser> additionalRecipients = await _userManager.GetUsersInRoleAsync(AuthRoles.CoverRecipient);

                    foreach (AppUser coverAdmin in additionalRecipients)
                    {
                        if (coverAdmin.IsStaffMember)
                        {
                            TeamMembershipResponse teacherEntry = new(
                                team.Id,
                                coverAdmin.Email,
                                TeamsMembershipLevel.Owner.Value);

                            if (returnData.All(value => value.EmailAddress != teacherEntry.EmailAddress))
                                returnData.Add(teacherEntry);
                        }
                    }
                }

                // Head Teachers
                Faculty faculty = await _facultyRepository.GetByOfferingId(offering.Id, cancellationToken);

                if (faculty is null)
                {
                    //error
                    _logger.Warning("Could not identify Faculty from offering {offering}.", offering.Name);

                    return Result.Failure<List<TeamMembershipResponse>>(new Error("Partners.Faculty.SearchFailed", $"Could not identify faculty from offering {offering.Name}."));
                }

                List<Staff> headTeachers = await _staffRepository.GetFacultyHeadTeachers(faculty.Id, cancellationToken);

                foreach (Staff teacher in headTeachers)
                {
                    TeamMembershipResponse entry = new(
                        team.Id,
                        teacher.EmailAddress,
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                        returnData.Add(entry);
                }

                if (_configuration is null) continue;
                
                Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

                if (course is null) continue;

                // Deputy Principals
                bool deputyPrincipals = _configuration.Contacts.DeputyPrincipalIds.TryGetValue(course.Grade, out List<string> deputyIds);

                if (deputyPrincipals is not false)
                {

                    foreach (var deputyId in deputyIds)
                    {
                        Staff deputyPrincipal = await _staffRepository.GetById(deputyId, cancellationToken);

                        if (deputyPrincipal is null) continue;

                        TeamMembershipResponse deputyEntry = new(
                            team.Id,
                            deputyPrincipal.EmailAddress,
                            TeamsMembershipLevel.Owner.Value);

                        if (returnData.All(value => value.EmailAddress != deputyEntry.EmailAddress))
                            returnData.Add(deputyEntry);
                    }
                }

                // Learning and Support Teachers
                bool learningSupport = _configuration.Contacts.LearningSupportIds.TryGetValue(course.Grade, out List<string> lastStaffIds);

                if (learningSupport is not false)
                {
                    foreach (string staffId in lastStaffIds)
                    {
                        Staff learningSupportTeacher = await _staffRepository.GetById(staffId, cancellationToken);

                        if (learningSupportTeacher is null) continue;

                        TeamMembershipResponse lastEntry = new(
                            team.Id,
                            learningSupportTeacher.EmailAddress,
                            TeamsMembershipLevel.Owner.Value);

                        if (returnData.All(value => value.EmailAddress != lastEntry.EmailAddress))
                            returnData.Add(lastEntry);
                    }
                }
            }
        }

        if (team.Description.Split(';').Contains("GTUT"))
        {
            // Group Tutorial Team which will have a group tutorial
            string teamCourse = team.Name.Split(" - ")[2];

            GroupTutorial tutorial = await _groupTutorialRepository.GetByName(teamCourse, cancellationToken);

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
            List<string> teacherIds = tutorial
                .Teachers
                .Where(member =>
                    !member.IsDeleted &&
                    member.EffectiveFrom <= DateOnly.FromDateTime(DateTime.Today) &&
                    (!member.EffectiveTo.HasValue || member.EffectiveTo.Value >= DateOnly.FromDateTime(DateTime.Today)))
                .Select(member => member.StaffId)
                .ToList();

            List<Staff> teachers = await _staffRepository.GetListFromIds(teacherIds, cancellationToken);

            foreach (Staff teacher in teachers)
            {
                TeamMembershipResponse entry = new(
                    team.Id,
                    teacher.EmailAddress,
                    TeamsMembershipLevel.Owner.Value);

                if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                    returnData.Add(entry);
            }
        }

        // Mandatory Owners
        List<string> mandatoryOwners = _teamsConfiguration.MandatoryOwnerIds;

        if (mandatoryOwners.Any())
        {
            foreach (string staffId in mandatoryOwners)
            {
                Staff mandatoryOwner = await _staffRepository.GetById(staffId, cancellationToken);

                if (mandatoryOwner is null) continue;

                TeamMembershipResponse lastEntry = new(
                    team.Id,
                    mandatoryOwner.EmailAddress,
                    TeamsMembershipLevel.Owner.Value);

                if (returnData.All(value => value.EmailAddress != lastEntry.EmailAddress))
                    returnData.Add(lastEntry);
            }
        }
        else
        {
            List<string> standardOwners = new()
            {
                "michael.necovski2@det.nsw.edu.au",
                "christopher.robertson@det.nsw.edu.au",
                "virginia.cluff@det.nsw.edu.au",
                //"scott.new@det.nsw.edu.au",
                "julie.dent@det.nsw.edu.au",
                "benjamin.hillsley@det.nsw.edu.au"
            };

            foreach (string owner in standardOwners)
            {
                TeamMembershipResponse entry = new(
                    team.Id,
                    owner,
                    TeamsMembershipLevel.Owner.Value);

                if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                    returnData.Add(entry);
            }
        }
        
        return returnData;
    }
}
