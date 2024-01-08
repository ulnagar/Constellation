namespace Constellation.Application.Schools.GetSchoolContactDetails;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
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
        School school = await _schoolRepository.GetById(request.Code, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(GetSchoolContactDetailsQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.Code), true)
                .Warning("Failed to retrieve contact details for school");

            return Result.Failure<SchoolContactDetailsResponse>(DomainErrors.Partners.School.NotFound(request.Code));
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