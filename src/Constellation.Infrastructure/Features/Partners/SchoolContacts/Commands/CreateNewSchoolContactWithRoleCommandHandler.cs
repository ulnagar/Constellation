namespace Constellation.Infrastructure.Features.Partners.SchoolContacts.Commands;

using Constellation.Application.Features.Partners.SchoolContacts.Commands;

public class CreateNewSchoolContactWithRoleCommandHandler : IRequestHandler<CreateNewSchoolContactWithRoleCommand>
{
    private readonly IMediator _mediator;

    public CreateNewSchoolContactWithRoleCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Unit> Handle(CreateNewSchoolContactWithRoleCommand request, CancellationToken cancellationToken)
    {
        var contactId = await _mediator.Send(new CreateNewSchoolContactCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailAddress = request.EmailAddress,
            SelfRegistered = request.SelfRegistered
        }, cancellationToken);

        await _mediator.Send(new CreateNewAssignmentForSchoolContactCommand
        {
            ContactId = contactId,
            SchoolCode = request.SchoolCode,
            Position = request.Position
        }, cancellationToken);

        return Unit.Value;
    }
}
