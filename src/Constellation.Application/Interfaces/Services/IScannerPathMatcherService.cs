namespace Constellation.Application.Interfaces.Services;

public interface IScannerPathMatcherService
{
    bool IsBlockedPath(string path);
}
