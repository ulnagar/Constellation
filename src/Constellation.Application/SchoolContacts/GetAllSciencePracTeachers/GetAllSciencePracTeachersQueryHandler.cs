namespace Constellation.Application.SchoolContacts.GetAllSciencePracTeachers;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Shared;
using Core.Models;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;
using Core.ValueObjects;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllSciencePracTeachersQueryHandler
    : IQueryHandler<GetAllSciencePracTeachersQuery, List<SchoolContactResponse>>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetAllSciencePracTeachersQueryHandler(
        ISchoolContactRepository contactRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<GetAllSciencePracTeachersQuery>();
    }

    public async Task<Result<List<SchoolContactResponse>>> Handle(GetAllSciencePracTeachersQuery request, CancellationToken cancellationToken)
    {
        List<SchoolContactResponse> response = new();

        List<SchoolContact> contacts = await _contactRepository.GetAllByRole(Position.SciencePracticalTeacher, cancellationToken);

        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        foreach (SchoolContact contact in contacts)
        {
            List<SchoolContactRole> activeAssignments = contact.Assignments
                .Where(assignment => 
                    !assignment.IsDeleted &&
                    assignment.Role == Position.SciencePracticalTeacher)
                .ToList();

            Result<Name> name = Name.Create(contact.FirstName, string.Empty, contact.LastName);
            Result<EmailAddress> email = EmailAddress.Create(contact.EmailAddress);

            foreach (SchoolContactRole assignment in activeAssignments)
            {
                School school = schools.FirstOrDefault(entry => entry.Code == assignment.SchoolCode);

                if (school is null)
                    continue;
                
                bool directNumber = false;
                PhoneNumber phone;

                if (string.IsNullOrWhiteSpace(contact.PhoneNumber))
                {
                    Result<PhoneNumber> phoneNumber = PhoneNumber.Create(school.PhoneNumber);
                    phone = phoneNumber.IsFailure ? PhoneNumber.Empty : phoneNumber.Value;
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
                    true,
                    assignment.Note,
                    contact.SelfRegistered));
            }
        }

        return response;
    }
}
