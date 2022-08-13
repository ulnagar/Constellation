namespace Constellation.Core.Refactor.Exceptions;

using System;

public class UnsupportedMSTeamTypeException : Exception
{
    public UnsupportedMSTeamTypeException(string code)
        : base($"Team Type \"{code}\" is unsupported.")
    {
    }
}
