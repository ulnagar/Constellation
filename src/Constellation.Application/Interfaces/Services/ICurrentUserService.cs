namespace Constellation.Application.Interfaces.Services;

public interface ICurrentUserService
{
    string UserName { get; }
    bool IsAuthenticated { get; }
}
