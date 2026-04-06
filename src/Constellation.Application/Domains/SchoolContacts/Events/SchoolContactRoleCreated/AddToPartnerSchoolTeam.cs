namespace Constellation.Application.Domains.SchoolContacts.Events.SchoolContactRoleCreated;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Events;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Enums;
using Interfaces.Repositories;
using Schools.Enums;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddToPartnerSchoolTeam
    : IDomainEventHandler<SchoolContactRoleCreatedDomainEvent>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddToPartnerSchoolTeam(
        ISchoolContactRepository contactRepository,
        ISchoolRepository schoolRepository,
        ITeamOperationRepository operationsRepository,
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
        SchoolContact? contact = await _contactRepository.GetById(notification.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(notification.ContactId), true)
                .Warning("Failed to add new School Contact to Partner Team");

            return;
        }

        List<SchoolCode> schoolCodes = contact.Assignments
            .Where(role => !role.IsDeleted)
            .Select(role => role.SchoolCode)
            .Distinct()
            .ToList();

        bool isPrimary = false;
        bool isSecondary = false;

        foreach (SchoolCode schoolCode in schoolCodes)
        {
            SchoolType type = await _schoolRepository.GetSchoolType(schoolCode, cancellationToken);

            if (type.Equals(SchoolType.Primary))
                isPrimary = true;

            if (type.Equals(SchoolType.Secondary))
                isSecondary = true;

            if (type.Equals(SchoolType.Central))
            {
                isPrimary = true;
                isSecondary = true;
            }
        }

        if (isPrimary)
        {
            ModifyTeamMembershipTeamOperation operation = new(
                MicrosoftTeam.PrimaryPartnerSchoolsTeamId,
                contact.EmailAddress,
                TeamAction.AddMember);

            _operationsRepository.Insert(operation);
        }

        if (isSecondary)
        {
            ModifyTeamMembershipTeamOperation operation = new(
                MicrosoftTeam.SecondaryPartnerSchoolsTeamId,
                contact.EmailAddress,
                TeamAction.AddMember);

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}