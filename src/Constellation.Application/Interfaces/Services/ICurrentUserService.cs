namespace Constellation.Application.Interfaces.Services;

public interface ICurrentUserService
{
    string UserName { get; }
    string EmailAddress { get; }
    bool IsAuthenticated { get; }
}
