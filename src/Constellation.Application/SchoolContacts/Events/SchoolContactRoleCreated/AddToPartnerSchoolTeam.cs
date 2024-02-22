namespace Constellation.Application.SchoolContacts.Events.SchoolContactRoleCreated;

using Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.SchoolContacts.Events;
using Constellation.Core.Models.SchoolContacts.Repositories;
using Core.Models.SchoolContacts.Errors;
using Core.Shared;
using Enums;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddToPartnerSchoolTeam
    : IDomainEventHandler<SchoolContactRoleCreatedDomainEvent>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddToPartnerSchoolTeam(
        ISchoolContactRepository contactRepository,
        ISchoolRepository schoolRepository,
        IMSTeamOperationsRepository operationsRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _schoolRepository = schoolRepository;
        _operationsRepository = operationsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<SchoolContactRoleCreatedDomainEvent>();
    }

    public async Task Handle(SchoolContactRoleCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(notification.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(notification.ContactId), true)
                .Warning("Failed to add new School Contact to Partner Team");

            return;
        }

        List<string> schoolCodes = contact.Assignments
            .Where(role => !role.IsDeleted)
            .Select(role => role.SchoolCode)
            .Distinct()
            .ToList();

        List<School> schools = await _schoolRepository.GetListFromIds(schoolCodes, cancellationToken);

        bool isPrimary = schools
            .SelectMany(school => 
                school.Students
                    .Where(student => !student.IsDeleted))
            .Any(student => student.CurrentGrade <= Grade.Y06);

        bool isSecondary = schools
            .SelectMany(school => 
                school.Students.Where(student => !student.IsDeleted))
            .Any(student => student.CurrentGrade >= Grade.Y07);

        if (isPrimary)
        {
            ContactAddedMSTeamOperation operation = new()
            {
                ContactId = contact.Id,
                DateScheduled = DateTime.Now,
                TeamName = MicrosoftTeam.PrimaryPartnerSchools,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Member
            };

            _operationsRepository.Insert(operation);
        }

        if (isSecondary)
        {
            ContactAddedMSTeamOperation operation = new()
            {
                ContactId = contact.Id,
                DateScheduled = DateTime.Now,
                TeamName = MicrosoftTeam.SecondaryPartnerSchools,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Member
            };

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}