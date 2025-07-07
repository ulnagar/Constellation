namespace Constellation.Application.Domains.Faculties.Queries.GetFacultyManagers;

using Abstractions.Messaging;
using Core.Models.Faculties;
using Core.Models.Faculties.Errors;
using Core.Models.Faculties.Repositories;
using Core.Models.Faculties.ValueObjects;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFacultyManagersQueryHandler
: IQueryHandler<GetFacultyManagersQuery, List<StaffMember>>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public GetFacultyManagersQueryHandler(
        IFacultyRepository facultyRepository,
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _facultyRepository = facultyRepository;
        _staffRepository = staffRepository;
        _logger = logger.ForContext<GetFacultyManagersQuery>();
    }

    public async Task<Result<List<StaffMember>>> Handle(GetFacultyManagersQuery request, CancellationToken cancellationToken)
    {
        Faculty faculty = await _facultyRepository.GetById(request.FacultyId, cancellationToken);

        if (faculty is null)
        {
            _logger
                .ForContext(nameof(GetFacultyManagersQuery), request, true)
                .ForContext(nameof(Error), FacultyErrors.NotFound(request.FacultyId), true)
                .Warning("Failed to retrieve list of Faculty managers");

            return Result.Failure<List<StaffMember>>(FacultyErrors.NotFound(request.FacultyId));
        }

        List<StaffId> facultyManagerIds = faculty
            .Members
            .Where(member =>
                !member.IsDeleted &&
                member.Role == FacultyMembershipRole.Manager)
            .Select(member => member.StaffId)
            .ToList();

        return await _staffRepository.GetListFromIds(facultyManagerIds, cancellationToken);
    }
}
