namespace Constellation.Application.StaffMembers.GetStaffList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Faculty.Repositories;
using Core.Models;
using Core.Models.Faculty;
using Core.Models.Faculty.Identifiers;
using Core.Shared;
using Faculties.GetFaculty;
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

        List<Staff> staff = await _staffRepository.GetAll(cancellationToken);

        List<Faculty> faculties = await _facultyRepository.GetAll(cancellationToken);

        List<School> schools = await _schoolRepository.GetAll(cancellationToken);

        foreach (Staff member in staff)
        {
            List<FacultyId> memberFacultyIds = member
                .Faculties
                .Where(faculty => !faculty.IsDeleted)
                .Select(faculty => faculty.FacultyId)
                .ToList();

            List<FacultyResponse> memberFaculties = faculties
                .Where(entry => memberFacultyIds.Contains(entry.Id))
                .Select(faculty => 
                    new FacultyResponse(
                        faculty.Id, 
                        faculty.Name, 
                        faculty.Colour))
                .ToList();

            School school = schools.FirstOrDefault(school => school.Code == member.SchoolCode);

            response.Add(new(
                member.StaffId,
                member.GetName(),
                memberFaculties,
                school?.Name,
                member.IsDeleted));
        }

        return response;
    }
}
