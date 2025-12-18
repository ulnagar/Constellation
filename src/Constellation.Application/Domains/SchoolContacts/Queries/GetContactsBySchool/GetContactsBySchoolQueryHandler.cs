namespace Constellation.Application.Domains.SchoolContacts.Queries.GetContactsBySchool;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Helpers;
using Interfaces.Repositories;
using Schools.Enums;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetContactsBySchoolQueryHandler
    : IQueryHandler<GetContactsBySchoolQuery, List<SchoolWithContactsResponse>>
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ILogger _logger;

    public GetContactsBySchoolQueryHandler(
        ISchoolRepository schoolRepository,
        ISchoolContactRepository contactRepository,
        ILogger logger)
    {
        _schoolRepository = schoolRepository;
        _contactRepository = contactRepository;
        _logger = logger.ForContext<GetContactsBySchoolQuery>();
    }

    public async Task<Result<List<SchoolWithContactsResponse>>> Handle(GetContactsBySchoolQuery request, CancellationToken cancellationToken)
    {
        List<SchoolWithContactsResponse> response = new();

        List<School> schools = await _schoolRepository.GetWithCurrentStudents(cancellationToken);

        foreach (School school in schools)
        {
            SchoolType schoolType = await _schoolRepository.GetSchoolType(school.Code, cancellationToken);
            
            List<SchoolWithContactsResponse.ContactDetails> entries = new();

            List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(school.Code, cancellationToken);

            foreach (SchoolContact contact in contacts)
            {
               PhoneNumber phone = contact.PhoneNumber == PhoneNumber.Empty
                    ? PhoneNumber.Create(school.PhoneNumber).Value
                    : contact.PhoneNumber;

                List<SchoolContactRole> roles = contact.Assignments
                    .Where(assignment =>
                        !assignment.IsDeleted &&
                        assignment.SchoolCode == school.Code)
                    .ToList();

                foreach (SchoolContactRole role in roles)
                {
                    if (!request.IncludeRestrictedContacts && role.IsContactRoleRestricted())
                        continue;

                    entries.Add(new(
                        contact.Id,
                        role.Id,
                        contact.Name,
                        contact.EmailAddress,
                        phone,
                        role.Role,
                        role.Note));
                }
            }

            response.Add(new(
                school.Code,
                school.Name,
                schoolType,
                entries));
        }

        return response;
    }
}
