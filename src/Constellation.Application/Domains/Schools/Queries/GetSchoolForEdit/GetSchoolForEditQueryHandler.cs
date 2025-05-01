namespace Constellation.Application.Domains.Schools.Queries.GetSchoolForEdit;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolForEditQueryHandler
: IQueryHandler<GetSchoolForEditQuery, SchoolEditResponse>
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetSchoolForEditQueryHandler(
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<GetSchoolForEditQuery>();
    }

    public async Task<Result<SchoolEditResponse>> Handle(GetSchoolForEditQuery request, CancellationToken cancellationToken)
    {
        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(GetSchoolForEditQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to retrieve School");

            return Result.Failure<SchoolEditResponse>(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }

        return new SchoolEditResponse(
            school.Code,
            school.Name,
            school.Address,
            school.Town,
            school.State,
            school.PostCode,
            school.PhoneNumber,
            school.FaxNumber,
            school.EmailAddress,
            school.Directorate,
            school.HeatSchool,
            school.EducationalServicesTeam,
            school.PrincipalNetwork,
            school.TimetableApplication,
            school.RollCallGroup);
    }
}
