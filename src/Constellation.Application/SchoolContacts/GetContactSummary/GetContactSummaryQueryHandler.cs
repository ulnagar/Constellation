namespace Constellation.Application.SchoolContacts.GetContactSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetContactSummaryQueryHandler
    : IQueryHandler<GetContactSummaryQuery, ContactSummaryResponse>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ILogger _logger;

    public GetContactSummaryQueryHandler(
        ISchoolContactRepository contactRepository,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _logger = logger.ForContext<GetContactSummaryQuery>();
    }

    public async Task<Result<ContactSummaryResponse>> Handle(GetContactSummaryQuery request, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(request.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger.Warning("Could not find School Contact with Id {id}", request.ContactId);

            return Result.Failure<ContactSummaryResponse>(DomainErrors.Partners.Contact.NotFound(request.ContactId));
        }

        return new ContactSummaryResponse(
            contact.Id,
            contact.FirstName,
            contact.LastName,
            contact.EmailAddress,
            contact.PhoneNumber);
    }
}
