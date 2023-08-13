namespace Constellation.Application.AdminDashboards.VerifySchoolContactAccess;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal class VerifySchoolContactAccessQueryHandler
    : IQueryHandler<VerifySchoolContactAccessQuery, UserAuditDto>
{
    private readonly IAuthService _authService;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ILogger _logger;

    public VerifySchoolContactAccessQueryHandler(
        IAuthService authService,
        ISchoolContactRepository contactRepository,
        ILogger logger)
    {
        _authService = authService;
        _contactRepository = contactRepository;
        _logger = logger.ForContext<VerifySchoolContactAccessQuery>();
    }

    public async Task<Result<UserAuditDto>> Handle(VerifySchoolContactAccessQuery request, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(request.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger.Warning("Could not find School Contact with Id {id}", request.ContactId);

            return Result.Failure<UserAuditDto>(DomainErrors.Partners.Contact.NotFound(request.ContactId));
        }

        return await _authService.VerifyContactAccess(contact.EmailAddress);
    }
}