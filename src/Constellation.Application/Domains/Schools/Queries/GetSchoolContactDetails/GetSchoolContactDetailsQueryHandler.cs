namespace Constellation.Application.Domains.Schools.Queries.GetSchoolContactDetails;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.Schools.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetSchoolContactDetailsQueryHandler 
    : IQueryHandler<GetSchoolContactDetailsQuery, SchoolContactDetailsResponse>
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetSchoolContactDetailsQueryHandler(
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<GetSchoolContactDetailsQuery>();
    }

    public async Task<Result<SchoolContactDetailsResponse>> Handle(GetSchoolContactDetailsQuery request, CancellationToken cancellationToken)
    {
        School? school = await _schoolRepository.GetById(request.Code, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(GetSchoolContactDetailsQuery), request, true)
                .ForContext(nameof(Error), SchoolErrors.NotFound(request.Code), true)
                .Warning("Failed to retrieve contact details for school");

            return Result.Failure<SchoolContactDetailsResponse>(SchoolErrors.NotFound(request.Code));
        }

        return new SchoolContactDetailsResponse(
            school.Code,
            school.Name,
            school.Address,
            school.Town,
            school.State,
            school.PostCode,
            school.PhoneNumber,
            school.FaxNumber,
            school.EmailAddress);
    }
}