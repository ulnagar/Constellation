namespace Constellation.Application.Domains.SchoolContacts.Queries.ExportContactsBySchool;

using Abstractions.Messaging;
using Application.Helpers;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Core.ValueObjects;
using DTOs;
using GetContactsBySchool;
using Helpers;
using Interfaces.Repositories;
using Interfaces.Services;
using Schools.Enums;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportContactsBySchoolQueryHandler
    : IQueryHandler<ExportContactsBySchoolQuery, FileDto>
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public ExportContactsBySchoolQueryHandler(
        ISchoolRepository schoolRepository,
        ISchoolContactRepository contactRepository,
        IExcelService excelService,
        ILogger logger)
    {
        _schoolRepository = schoolRepository;
        _contactRepository = contactRepository;
        _excelService = excelService;
        _logger = logger.ForContext<ExportContactsBySchoolQuery>();
    }

    public async Task<Result<FileDto>> Handle(ExportContactsBySchoolQuery request, CancellationToken cancellationToken)
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
                        name.Value,
                        email.Value,
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

        MemoryStream stream = await _excelService.CreateSchoolContactExport(response, cancellationToken);

        FileDto file = new()
        {
            FileData = stream.ToArray(),
            FileName = "School Contacts List.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }
}
