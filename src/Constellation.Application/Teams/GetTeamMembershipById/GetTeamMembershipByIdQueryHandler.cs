namespace Constellation.Application.Teams.GetTeamMembershipById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Core.Models.Offerings;
using Core.Models.Offerings.ValueObjects;
using Core.Models.Subjects;
using Org.BouncyCastle.Cms;
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
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger _logger;

    public GetTeamMembershipByIdQueryHandler(
        ITeamRepository teamRepository,
        IOfferingRepository offeringRepository,
        IFacultyRepository facultyRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IGroupTutorialRepository groupTutorialRepository,
        ICourseRepository courseRepository,
        Serilog.ILogger logger,
        IClassCoverRepository coverRepository)
    {
        _teamRepository = teamRepository;
        _offeringRepository = offeringRepository;
        _facultyRepository = facultyRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _coverRepository = coverRepository;
        _groupTutorialRepository = groupTutorialRepository;
        _courseRepository = courseRepository;
        _logger = logger.ForContext<GetTeamMembershipByIdQuery>();
    }

    public async Task<Result<List<TeamMembershipResponse>>> Handle(GetTeamMembershipByIdQuery request, CancellationToken cancellationToken)
    {
        List<TeamMembershipResponse> returnData = new List<TeamMembershipResponse>();

        Team team = await _teamRepository.GetById(request.Id, cancellationToken);

        if (team is null)
        {
            _logger.Warning("Error: Task failed with error {@error}", DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);

            return Result.Failure<List<TeamMembershipResponse>>(DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);
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
                    TeamMembershipResponse entry = new(
                        team.Id,
                        student.EmailAddress,
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

                    TeamMembershipResponse cathyEntry = new(
                        team.Id,
                        "catherine.crouch@det.nsw.edu.au",
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != cathyEntry.EmailAddress))
                        returnData.Add(cathyEntry);

                    TeamMembershipResponse karenEntry = new(
                        team.Id,
                        "karen.bellamy3@det.nsw.edu.au",
                        TeamsMembershipLevel.Owner.Value);

                    if (returnData.All(value => value.EmailAddress != karenEntry.EmailAddress))
                        returnData.Add(karenEntry);
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

                Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

                if (course is not null)
                {
                    Staff deputyMiddleSchool, deputySecondarySchool;

                    switch (course.Grade)
                    {
                        case Grade.Y05:
                        case Grade.Y06:
                            deputyMiddleSchool = await _staffRepository.GetById("1102472", cancellationToken);

                            if (deputyMiddleSchool is not null)
                            {
                                TeamMembershipResponse entry = new(
                                    team.Id,
                                    deputyMiddleSchool.EmailAddress,
                                    TeamsMembershipLevel.Owner.Value);

                                if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                                    returnData.Add(entry);
                            }

                            break;
                        case Grade.Y07:
                            deputyMiddleSchool = await _staffRepository.GetById("1102472", cancellationToken);

                            if (deputyMiddleSchool is not null)
                            {
                                TeamMembershipResponse entry = new(
                                    team.Id,
                                    deputyMiddleSchool.EmailAddress,
                                    TeamsMembershipLevel.Owner.Value);

                                if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                                    returnData.Add(entry);
                            }

                            deputySecondarySchool = await _staffRepository.GetById("1055721", cancellationToken);

                            if (deputySecondarySchool is not null)
                            {
                                TeamMembershipResponse entry = new(
                                    team.Id,
                                    deputySecondarySchool.EmailAddress,
                                    TeamsMembershipLevel.Owner.Value);

                                if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                                    returnData.Add(entry);
                            }

                            break;
                        case Grade.Y08:
                        case Grade.Y09:
                        case Grade.Y10:
                        case Grade.Y11:
                        case Grade.Y12:
                            deputySecondarySchool = await _staffRepository.GetById("1055721", cancellationToken);

                            if (deputySecondarySchool is not null)
                            {
                                TeamMembershipResponse entry = new(
                                    team.Id,
                                    deputySecondarySchool.EmailAddress,
                                    TeamsMembershipLevel.Owner.Value);

                                if (returnData.All(value => value.EmailAddress != entry.EmailAddress))
                                    returnData.Add(entry);
                            }

                            break;
                    }
                }
            }
        }

        if (team.Description.Split(';').Contains("GTUT"))
        {
            // Group Tutorial Team which will have a group tutorial
            string teamCourse = team.Name.Split(" - ")[2];

            GroupTutorial tutorial = await _groupTutorialRepository.GetByName(teamCourse);

            if (tutorial is null)
            {
                _logger.Warning("Could not identify Group Tutorial from name {name}", teamCourse);

                return Result.Failure<List<TeamMembershipResponse>>(new Error("GroupTutorials.GroupTutorial.NotFound", $"Could not identify Group Tutorial from name {teamCourse}."));
            }

            // Enrolled Students

            List<string> studentIds = tutorial
                .CurrentEnrolments
                .Select(enrol => enrol.StudentId)
                .ToList();

            List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

            foreach (Student student in students)
            {
                TeamMembershipResponse entry = new(
                    team.Id,
                    student.EmailAddress,
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
        List<string> standardOwners = new()
        {
            "michael.necovski2@det.nsw.edu.au",
            "christopher.robertson@det.nsw.edu.au",
            "virginia.cluff@det.nsw.edu.au",
            "scott.new@det.nsw.edu.au",
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

        return returnData;
    }
}
