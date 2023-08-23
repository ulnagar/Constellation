namespace Constellation.Application.Teams.GetTeamMembershipById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
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
    private readonly ILogger _logger;

    public GetTeamMembershipByIdQueryHandler(
        ITeamRepository teamRepository,
        IOfferingRepository offeringRepository,
        IFacultyRepository facultyRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IGroupTutorialRepository groupTutorialRepository,
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
        _logger = logger.ForContext<GetTeamMembershipByIdQuery>();
    }

    public async Task<Result<List<TeamMembershipResponse>>> Handle(GetTeamMembershipByIdQuery request, CancellationToken cancellationToken)
    {
        var returnData = new List<TeamMembershipResponse>();

        var team = await _teamRepository.GetById(request.Id, cancellationToken);

        if (team is null)
        {
            _logger.Warning("Error: Task failed with error {@error}", DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);

            return Result.Failure<List<TeamMembershipResponse>>(DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);
        }

        if (team.Description.Split(';').Contains("CLASS"))
        {
            // Class Team which should have an offering

            _ = int.TryParse(team.Name.Split(" - ")[1], out int teamYear);
            var teamCourse = team.Name.Split(" - ")[2];

            var course = await _offeringRepository.GetFromYearAndName(teamYear, teamCourse);

            if (course is null)
            {
                //error
                _logger.Warning("Could not identify Offering from year {year} and name {name}", teamYear, teamCourse);

                return Result.Failure<List<TeamMembershipResponse>>(OfferingErrors.SearchFailed(teamYear, teamCourse));
            }

            // Enrolled Students

            var students = await _studentRepository.GetCurrentEnrolmentsForOffering(course.Id, cancellationToken);

            foreach (var student in students)
            {
                var entry = new TeamMembershipResponse(
                    team.Id,
                    student.EmailAddress,
                    TeamsMembershipLevel.Member.Value);

                if (!returnData.Any(value => value.EmailAddress == entry.EmailAddress))
                    returnData.Add(entry);
            }

            // Class Teachers

            var teachers = await _staffRepository.GetCurrentTeachersForOffering(course.Id, cancellationToken);

            foreach (var teacher in teachers)
            {
                var entry = new TeamMembershipResponse(
                    team.Id,
                    teacher.EmailAddress,
                    TeamsMembershipLevel.Owner.Value);

                if (!returnData.Any(value => value.EmailAddress == entry.EmailAddress))
                    returnData.Add(entry);
            }

            // Covering Teachers

            var coveringTeachers = await _coverRepository.GetCurrentCoveringTeachersForOffering(course.Id, cancellationToken);

            foreach (var teacher in coveringTeachers)
            {
                var entry = new TeamMembershipResponse(
                    team.Id,
                    teacher,
                    TeamsMembershipLevel.Owner.Value);

                if (!returnData.Any(value => value.EmailAddress == entry.EmailAddress))
                    returnData.Add(entry);

                var cathyEntry = new TeamMembershipResponse(
                    team.Id,
                    "catherine.crouch@det.nsw.edu.au",
                    TeamsMembershipLevel.Owner.Value);

                if (!returnData.Any(value => value.EmailAddress == cathyEntry.EmailAddress))
                    returnData.Add(cathyEntry);

                var karenEntry = new TeamMembershipResponse(
                    team.Id,
                    "karen.bellamy3@det.nsw.edu.au",
                    TeamsMembershipLevel.Owner.Value);

                if (!returnData.Any(value => value.EmailAddress == karenEntry.EmailAddress))
                    returnData.Add(karenEntry);
            }

            // Head Teachers

        var faculty = await _facultyRepository.GetByOfferingId(course.Id, cancellationToken);

            if (faculty is null)
            {
                //error
                _logger.Warning("Could not identify Faculty from offering {course}.", course.Name);

                return Result.Failure<List<TeamMembershipResponse>>(new Error("Partners.Faculty.SearchFailed", $"Could not identify faculty from offering {course.Name}."));
            }

            var headTeachers = await _staffRepository.GetFacultyHeadTeachers(faculty.Id, cancellationToken);

            foreach (var teacher in headTeachers)
            {
                var entry = new TeamMembershipResponse(
                    team.Id,
                    teacher.EmailAddress,
                    TeamsMembershipLevel.Owner.Value);

                if (!returnData.Any(value => value.EmailAddress == entry.EmailAddress))
                    returnData.Add(entry);
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

                if (!returnData.Any(value => value.EmailAddress == entry.EmailAddress))
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

                if (!returnData.Any(value => value.EmailAddress == entry.EmailAddress))
                    returnData.Add(entry);
            }
        }

        // Mandatory Owners

        List<string> standardOwners = new()
        {
            "michael.necovski2@det.nsw.edu.au",
            "christopher.robertson@det.nsw.edu.au",
            "carolyn.hungerford@det.nsw.edu.au",
            "virginia.cluff@det.nsw.edu.au",
            "scott.new@det.nsw.edu.au",
            "julie.dent@det.nsw.edu.au",
            "LUISA.Simeonidis5@det.nsw.edu.au",
            "danielle.latinovic@det.nsw.edu.au",
            "benjamin.hillsley@det.nsw.edu.au"
        };

        foreach (string owner in standardOwners)
        {
            TeamMembershipResponse entry = new(
                team.Id,
                owner,
                TeamsMembershipLevel.Owner.Value);

            if (!returnData.Any(value => value.EmailAddress == entry.EmailAddress))
                returnData.Add(entry);
        }

        return returnData;
    }
}
