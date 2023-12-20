namespace Constellation.Application.Schools.GetSchoolById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Core.Errors;
using Core.Models;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolByIdQueryHandler
    : IQueryHandler<GetSchoolByIdQuery, SchoolResponse>
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetSchoolByIdQueryHandler(
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<GetSchoolByIdQuery>();
    }

    public async Task<Result<SchoolResponse>> Handle(GetSchoolByIdQuery request, CancellationToken cancellationToken)
    {
        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(GetSchoolByIdQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to retrieve school");

            return Result.Failure<SchoolResponse>(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }

        return new SchoolResponse(school.Code, school.Name);
    }
}
