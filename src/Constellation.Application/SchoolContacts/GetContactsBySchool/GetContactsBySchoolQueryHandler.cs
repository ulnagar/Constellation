namespace Constellation.Application.SchoolContacts.GetContactsBySchool;

using Abstractions.Messaging;
using Constellation.Application.SchoolContacts.Helpers;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Core.ValueObjects;
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
                Result<Name> name = Name.Create(contact.FirstName, string.Empty, contact.LastName);

                if (name.IsFailure)
                {
                    _logger
                        .ForContext("Contact.FirstName", contact.FirstName)
                        .ForContext("Contact.LastName", contact.LastName)
                        .ForContext(nameof(Error), name.Error, true)
                        .Warning("Failed to retrieve list of School with active Contacts");

                    continue;
                }

                Result<EmailAddress> email = EmailAddress.Create(contact.EmailAddress);

                if (email.IsFailure)
                {
                    _logger
                        .ForContext("Contact.EmailAddress", contact.EmailAddress)
                        .ForContext(nameof(Error), email.Error, true)
                        .Warning("Failed to retrieve list of School with active Contacts");

                    continue;
                }

                Result<PhoneNumber> phone = string.IsNullOrWhiteSpace(contact.PhoneNumber)
                    ? PhoneNumber.Create(school.PhoneNumber)
                    : PhoneNumber.Create(contact.PhoneNumber);

                if (phone.IsFailure)
                {
                    _logger
                        .ForContext("Contact.PhoneNumber", contact.PhoneNumber)
                        .ForContext("School.PhoneNumber", school.PhoneNumber)
                        .ForContext(nameof(Error), phone.Error, true)
                        .Warning("Failed to retrieve list of School with active Contacts");
                }

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
                        name.Value,
                        email.Value,
                        phone.IsFailure ? PhoneNumber.Empty : phone.Value,
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
