namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentUser.Commands.RegisterNewUser;

using Abstractions.Messaging;
using Core.ValueObjects;

public sealed record RegisterNewUserCommand(
    Name UserName,
    EmailAddress Email,
    PhoneNumber? Mobile = null)
    : ICommand;
