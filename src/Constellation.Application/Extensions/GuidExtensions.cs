namespace Constellation.Application.Extensions;

using System;

public static class GuidExtensions
{
    public static string ToShortString(this Guid guid) => guid.ToString().Split('-')[0];
}