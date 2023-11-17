namespace Constellation.Application.Faculties.GetFacultyDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Faculties.UpdateFaculty;
using Constellation.Core.Models.Faculty.Errors;
using Core.Errors;
using Core.Models;
using Core.Models.Faculty;
using Core.Models.Faculty.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFacultyDetailsQueryHandler
    : IQueryHandler<GetFacultyDetailsQuery, FacultyDetailsResponse>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public GetFacultyDetailsQueryHandler(
        IFacultyRepository facultyRepository,
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _facultyRepository = facultyRepository;
        _staffRepository = staffRepository;
        _logger = logger.ForContext<GetFacultyDetailsQuery>();
    }

    public async Task<Result<FacultyDetailsResponse>> Handle(GetFacultyDetailsQuery request, CancellationToken cancellationToken)
    {
        Faculty faculty = await _facultyRepository.GetById(request.FacultyId, cancellationToken);

        if (faculty is null)
        {
            _logger
                .ForContext(nameof(UpdateFacultyCommand), request, true)
                .ForContext(nameof(Error), FacultyErrors.NotFound(request.FacultyId), true)
                .Warning("Failed to retrieve faculty details");

            return Result.Failure<FacultyDetailsResponse>(FacultyErrors.NotFound(request.FacultyId));
        }

        List<FacultyDetailsResponse.MemberEntry> staffMembers = new();

        foreach (FacultyMembership member in faculty.Members.Where(member => !member.IsDeleted))
        {
            Staff staff = await _staffRepository.GetById(member.StaffId, cancellationToken);

            if (staff is null)
            {
                _logger
                    .ForContext(nameof(UpdateFacultyCommand), request, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(member.StaffId), true)
                    .Warning("Failed to retrieve faculty details");

                return Result.Failure<FacultyDetailsResponse>(DomainErrors.Partners.Staff.NotFound(member.StaffId));
            }

            Result<Name> staffName = Name.Create(staff.FirstName, string.Empty, staff.LastName);

            if (staffName.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpdateFacultyCommand), request, true)
                    .ForContext(nameof(Error), staffName.Error, true)
                    .Warning("Failed to retrieve faculty details");

                return Result.Failure<FacultyDetailsResponse>(staffName.Error);
            }

            staffMembers.Add(new(
                member.Id,
                member.StaffId,
                staffName.Value,
                member.Role));
        }

        FacultyDetailsResponse response = new(
            faculty.Id,
            faculty.Name,
            faculty.Colour,
            staffMembers);

        return response;
    }
}