namespace Constellation.Application.Domains.EnrolmentContext.Interfaces;

using Core.Shared;
using Core.ValueObjects;

public interface IEnrolmentAuthService
{
    Task<Result> SendLoginTokenViaEmail(EmailAddress email, CancellationToken cancellationToken = default);
    Task<Result> SendLoginTokenViaSms(PhoneNumber number, CancellationToken cancellationToken = default);
    Task<Result> RegisterNewUser(Name name, EmailAddress emailAddress, PhoneNumber mobile, CancellationToken cancellationToken = default);
}