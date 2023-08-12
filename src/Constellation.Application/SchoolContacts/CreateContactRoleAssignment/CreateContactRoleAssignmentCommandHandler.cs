namespace Constellation.Infrastructure.Features.Partners.SchoolContacts.Commands;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Features.Partners.SchoolContacts.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.SchoolContacts.CreateContactRoleAssignment;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using MediatR;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateContactRoleAssignmentCommandHandler
    : ICommandHandler<CreateContactRoleAssignmentCommand>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ISchoolContactRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public CreateContactRoleAssignmentCommandHandler(
        ISchoolContactRepository contactRepository,
        ISchoolContactRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ILogger logger,
        IMediator mediator)
    {
        _contactRepository = contactRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger.ForContext<CreateContactRoleAssignmentCommand>();
    }

    public async Task<Result> Handle(CreateContactRoleAssignmentCommand request, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(request.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger.Warning("Could not find School Contact with Id {id}", request.ContactId);

            return Result.Failure(DomainErrors.Partners.Contact.NotFound(request.ContactId));
        }

        if (contact.IsDeleted)
        {
            contact.DateDeleted = null;
            contact.IsDeleted = false;
        }

        bool currentRoles = await _roleRepository.Exists(
            request.ContactId,
            request.SchoolCode,
            request.Position,
            cancellationToken);

        if (currentRoles)
            return Result.Success();

        SchoolContactRole assignment = new()
        {
            SchoolContactId = request.ContactId,
            SchoolCode = request.SchoolCode,
            Role = request.Position,
            DateEntered = DateTime.Now
        };

        contact.Assignments.Add(assignment);
        await _unitOfWork.CompleteAsync(cancellationToken);

        await _mediator.Publish(new SchoolContactRoleAssignmentCreatedNotification { AssignmentId = assignment.Id }, cancellationToken);
    
        return Result.Success();
    }
}
