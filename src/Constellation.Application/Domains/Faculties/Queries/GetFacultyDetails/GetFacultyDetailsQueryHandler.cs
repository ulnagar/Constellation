namespace Constellation.Application.Domains.Faculties.Queries.GetFacultyDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Faculties.Commands.UpdateFaculty;
using Core.Errors;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Errors;
using Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Core.ValueObjects;
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
            StaffMember staff = await _staffRepository.GetById(member.StaffId, cancellationToken);

            if (staff is null)
            {
                _logger
                    .ForContext(nameof(UpdateFacultyCommand), request, true)
                    .ForContext(nameof(Error), StaffMemberErrors.NotFound(member.StaffId), true)
                    .Warning("Failed to retrieve faculty details");

                return Result.Failure<FacultyDetailsResponse>(StaffMemberErrors.NotFound(member.StaffId));
            }

            staffMembers.Add(new(
                member.Id,
                member.StaffId,
                staff.Name,
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