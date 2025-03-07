namespace Constellation.Application.SchoolContacts.GetContactsWithRoleFromSchool;

using Abstractions.Messaging;
using Constellation.Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetContactsWithRoleFromSchoolQueryHandler 
    : IQueryHandler<GetContactsWithRoleFromSchoolQuery, List<ContactResponse>>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ILogger _logger;

    public GetContactsWithRoleFromSchoolQueryHandler(
        ISchoolContactRepository contactRepository,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _logger = logger.ForContext<GetContactsWithRoleFromSchoolQuery>();
    }
    
    public async Task<Result<List<ContactResponse>>> Handle(GetContactsWithRoleFromSchoolQuery request, CancellationToken cancellationToken)
    {
        List<ContactResponse> response = new();

        List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(request.Code, cancellationToken);

        foreach (SchoolContact contact in contacts)
        {
            foreach (SchoolContactRole role in contact.Assignments)
            {
                if (role.IsDeleted)
                    continue;

                response.Add(new(
                    contact.Id,
                    role.Id,
                    contact.FirstName,
                    contact.LastName,
                    contact.PhoneNumber,
                    contact.EmailAddress,
                    role.Role));
            }
        }

        return response;
    }
}