namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffList;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Faculties.Queries.GetFaculty;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffListQueryHandler
    : IQueryHandler<GetStaffListQuery, List<StaffResponse>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetStaffListQueryHandler(
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<GetStaffListQuery>();
    }

    public async Task<Result<List<StaffResponse>>> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
    {
        List<StaffResponse> response = new();

        List<StaffMember> staff = await _staffRepository.GetAll(cancellationToken);

        List<Faculty> faculties = await _facultyRepository.GetAll(cancellationToken);

        List<School> schools = await _schoolRepository.GetAll(cancellationToken);

        foreach (StaffMember member in staff)
        {
            List<FacultyResponse> memberFaculties = faculties
                .Where(entry => entry.Members.Any(membership => membership.StaffId == member.Id && !membership.IsDeleted))
                .Select(faculty => 
                    new FacultyResponse(
                        faculty.Id, 
                        faculty.Name, 
                        faculty.Colour))
                .ToList();

            School school = member.CurrentAssignment is not null
                ? schools.FirstOrDefault(school => school.Code == member.CurrentAssignment.SchoolCode)
                : null;

            string edvalCode = member.SystemLinks.FirstOrDefault(entry => entry.System == SystemType.Edval)?.Value ??
                               string.Empty;

            response.Add(new(
                member.Id,
                member.EmployeeId,
                member.Name,
                memberFaculties,
                school?.Name,
                edvalCode,
                member.IsDeleted));
        }

        return response;
    }
}
