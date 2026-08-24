namespace Constellation.Infrastructure.Services;

using Application.Interfaces.Services;
using Constellation.Application.Interfaces.Configuration;
using Microsoft.Extensions.Options;
using System;

internal sealed class ScannerPathMatcherService : IScannerPathMatcherService
{
    private readonly ScannerBlocklistOptions _options;

    public ScannerPathMatcherService(
        IOptions<ScannerBlocklistOptions> options)
    {
        _options = options.Value;
    }

    public bool IsBlockedPath(string path)
    {
        if (!_options.Enabled || string.IsNullOrEmpty(path))
            return false;

        foreach (var fragment in _options.PathFragments)
        {
            if (path.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        foreach (var ext in _options.Extensions)
        {
            if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
