namespace Constellation.Application.Features.Auth.Command;

using MediatR;

public class RegisterParentContactAsUserCommand : IRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
}
