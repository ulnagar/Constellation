namespace Constellation.Application.Domains.SchoolContacts.Queries.GetAllContacts;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Helpers;
using Interfaces.Repositories;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllContactsQueryHandler 
    : IQueryHandler<GetAllContactsQuery, List<SchoolContactResponse>>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public GetAllContactsQueryHandler(
        ISchoolContactRepository contactRepository,
        ISchoolRepository schoolRepository,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _schoolRepository = schoolRepository;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<GetAllContactsQuery>();
    }

    public async Task<Result<List<SchoolContactResponse>>> Handle(GetAllContactsQuery request, CancellationToken cancellationToken)
    {
        List<SchoolContactResponse> response = new();

        List<SchoolContact> contacts = request.Filter switch
        {
            GetAllContactsQuery.SchoolContactFilter.All => await _contactRepository.GetAll(cancellationToken),
            GetAllContactsQuery.SchoolContactFilter.WithRole => await _contactRepository.GetAllWithRole(cancellationToken),
            GetAllContactsQuery.SchoolContactFilter.WithoutRole => await _contactRepository.GetAllWithoutRole(cancellationToken),
            _ => await _contactRepository.GetAllWithRole(cancellationToken)
        };

        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        foreach (SchoolContact contact in contacts)
        {
            List<SchoolContactRole> activeAssignments = contact.Assignments
                .Where(assignment => !assignment.IsDeleted)
                .ToList();
            
            Result<Name> name = Name.Create(contact.FirstName, string.Empty, contact.LastName);
            if (name.IsFailure)
            {
                _logger
                    .ForContext(nameof(SchoolContact), contact, true)
                    .ForContext(nameof(Error), name.Error, true)
                    .Warning("Failed to create Name for School Contact by user {User}", _currentUserService.UserName);
            }

            Result<EmailAddress> email = EmailAddress.Create(contact.EmailAddress);
            if (email.IsFailure)
            {
                _logger
                    .ForContext(nameof(SchoolContact), contact, true)
                    .ForContext(nameof(Error), email.Error, true)
                    .Warning("Failed to create EmailAddress for School Contact by user {User}", _currentUserService.UserName);
            }

            if (activeAssignments.Count == 0)
            {
                response.Add(new SchoolContactResponse(
                    contact.Id,
                    SchoolContactRoleId.Empty, 
                    name.IsSuccess ? name.Value : null,
                    email.IsSuccess ? email.Value : EmailAddress.None,
                    PhoneNumber.Empty, 
                    true,
                    Position.Empty,
                    string.Empty,
                    false,
                    string.Empty,
                    contact.SelfRegistered));

                continue;
            }

            foreach (SchoolContactRole assignment in activeAssignments)
            {
                // If the request should not include restricted roles, ignore restricted roles.
                if (!request.IncludeRestrictedRoles && assignment.IsContactRoleRestricted())
                    continue;

                School school = schools.FirstOrDefault(entry => entry.Code == assignment.SchoolCode);

                bool directNumber = false;
                PhoneNumber phone;

                if (string.IsNullOrWhiteSpace(contact.PhoneNumber))
                {
                    if (school is null)
                    {
                        phone = PhoneNumber.Empty;
                    }
                    else
                    {
                        Result<PhoneNumber> phoneNumber = PhoneNumber.Create(school.PhoneNumber);
                        phone = phoneNumber.IsFailure ? PhoneNumber.Empty : phoneNumber.Value;
                    }
                }
                else
                {
                    Result<PhoneNumber> phoneNumber = PhoneNumber.Create(contact.PhoneNumber);
                    phone = phoneNumber.IsFailure ? PhoneNumber.Empty : phoneNumber.Value;
                    directNumber = true;
                }

                response.Add(new(
                    contact.Id,
                    assignment.Id,
                    name.Value,
                    email.Value,
                    phone,
                    directNumber,
                    assignment.Role,
                    assignment.SchoolName,
                    school is not null,
                    assignment.Note,
                    contact.SelfRegistered));
            }
        }

        return response;
    }
}
