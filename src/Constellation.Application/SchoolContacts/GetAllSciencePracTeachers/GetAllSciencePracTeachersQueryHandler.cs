namespace Constellation.Application.SchoolContacts.GetAllSciencePracTeachers;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Models.SchoolContacts.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllSciencePracTeachersQueryHandler
    : IQueryHandler<GetAllSciencePracTeachersQuery, List<ContactResponse>>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ILogger _logger;

    public GetAllSciencePracTeachersQueryHandler(
        ISchoolContactRepository contactRepository,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _logger = logger.ForContext<GetAllSciencePracTeachersQuery>();
    }

    public async Task<Result<List<ContactResponse>>> Handle(GetAllSciencePracTeachersQuery request, CancellationToken cancellationToken)
    {
        List<ContactResponse> responses = new();

        List<SchoolContact> contacts = await _contactRepository.GetAllByRole(SchoolContactRole.SciencePrac, cancellationToken);

        foreach (SchoolContact contact in contacts)
        {
            Result<Name> nameRequest = Name.Create(contact.FirstName, string.Empty, contact.LastName);

            if (nameRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(SchoolContact), contact, true)
                    .ForContext(nameof(Result.Error), nameRequest.Error, true)
                    .Warning("Could not create name for school contact");

                continue;
            }

            Result<EmailAddress> emailRequest = EmailAddress.Create(contact.EmailAddress);

            if (emailRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(SchoolContact), contact, true)
                    .ForContext(nameof(Result.Error), emailRequest.Error, true)
                    .Warning("Could not create email address for school contact");

                continue;
            }

            Result<PhoneNumber> phoneRequest = PhoneNumber.Create(contact.PhoneNumber);

            if (phoneRequest.IsFailure && !string.IsNullOrWhiteSpace(contact.PhoneNumber)) 
            {
                _logger
                    .ForContext(nameof(SchoolContact), contact, true)
                    .ForContext(nameof(Result.Error), phoneRequest.Error, true)
                    .Warning("Could not create phone number for school contact");

                continue;
            }

            foreach (SchoolContactRole assignment in contact.Assignments)
            {
                if (assignment.Role != SchoolContactRole.SciencePrac)
                    continue;

                responses.Add(new(
                    contact.Id,
                    assignment.Id,
                    nameRequest.Value,
                    emailRequest.Value,
                    phoneRequest.IsSuccess ? phoneRequest.Value : null,
                    contact.SelfRegistered,
                    assignment.SchoolCode,
                    assignment.SchoolName));
            }
        }

        return responses;
    }
}
