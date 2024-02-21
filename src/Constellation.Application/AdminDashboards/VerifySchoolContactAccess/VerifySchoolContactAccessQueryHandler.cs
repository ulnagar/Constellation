namespace Constellation.Application.AdminDashboards.VerifySchoolContactAccess;

using Abstractions.Messaging;
using DTOs;
using Interfaces.Services;
using Constellation.Core.Models.SchoolContacts;
using Core.Shared;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
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

        if (contact is not null) 
            return await _authService.VerifyContactAccess(contact.EmailAddress);
        
        _logger.Warning("Could not find School Contact with Id {id}", request.ContactId);

        return Result.Failure<UserAuditDto>(SchoolContactErrors.NotFound(request.ContactId));
    }
}