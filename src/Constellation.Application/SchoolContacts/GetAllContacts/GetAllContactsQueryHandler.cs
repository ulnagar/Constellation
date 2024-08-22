namespace Constellation.Application.SchoolContacts.GetAllContacts;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using GroupTutorials.GenerateTutorialAttendanceReport;
using Interfaces.Repositories;
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
    private readonly ILogger _logger;

    public GetAllContactsQueryHandler(
        ISchoolContactRepository contactRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _schoolRepository = schoolRepository;
        _logger = logger;
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

            if (activeAssignments.Count == 0)
            {
                response.Add(new SchoolContactResponse(
                    contact.Id,
                    SchoolContactRoleId.Empty, 
                    contact.DisplayName,
                    contact.EmailAddress,
                    contact.PhoneNumber,
                    true,
                    string.Empty,
                    string.Empty,
                    false,
                    string.Empty));

                continue;
            }

            foreach (SchoolContactRole assignment in activeAssignments)
            {
                School school = schools.FirstOrDefault(entry => entry.Code == assignment.SchoolCode);

                bool directNumber = false;
                string phoneNumber;

                if (string.IsNullOrWhiteSpace(contact.PhoneNumber))
                {
                    phoneNumber = school?.PhoneNumber;
                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                        directNumber = false;
                }
                else
                {
                    phoneNumber = contact.PhoneNumber;
                    directNumber = true;
                }

                response.Add(new(
                    contact.Id,
                    assignment.Id,
                    contact.DisplayName,
                    contact.EmailAddress,
                    phoneNumber,
                    directNumber,
                    assignment.Role,
                    assignment.SchoolName,
                    school is not null,
                    assignment.Note));
            }
        }

        return response;
    }
}
