namespace Constellation.Application.Teams.GetTeamMembershipById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTeamMembershipByIdQueryHandler
    : IQueryHandler<GetTeamMembershipByIdQuery, List<TeamMembershipResponse>>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IClassCoverRepository _coverRepository;

    public GetTeamMembershipByIdQueryHandler(
        ITeamRepository teamRepository,
        ICourseOfferingRepository offeringRepository,
        IFacultyRepository facultyRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IClassCoverRepository coverRepository)
    {
        _teamRepository = teamRepository;
        _offeringRepository = offeringRepository;
        _facultyRepository = facultyRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _coverRepository = coverRepository;
    }

    public async Task<Result<List<TeamMembershipResponse>>> Handle(GetTeamMembershipByIdQuery request, CancellationToken cancellationToken)
    {
        var returnData = new List<TeamMembershipResponse>();

        var team = await _teamRepository.GetById(request.Id, cancellationToken);

        if (team is null)
        {
            return Result.Failure<List<TeamMembershipResponse>>(DomainErrors.LinkedSystems.Teams.TeamNotFoundInDatabase);
        }

        _ = int.TryParse(team.Name.Split(" - ")[1], out int teamYear);
        var teamCourse = team.Name.Split(" - ")[2];

        var course = await _offeringRepository.GetFromYearAndName(teamYear, teamCourse);

        if (course is null)
        {
            //error
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
            // error
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

        // Mandatory Owners

        var standardOwners = new List<string>
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

        foreach (var owner in standardOwners)
        {
            var entry = new TeamMembershipResponse(
                team.Id,
                owner,
                TeamsMembershipLevel.Owner.Value);

            if (!returnData.Any(value => value.EmailAddress == entry.EmailAddress))
                returnData.Add(entry);
        }

        return returnData;
    }
}
