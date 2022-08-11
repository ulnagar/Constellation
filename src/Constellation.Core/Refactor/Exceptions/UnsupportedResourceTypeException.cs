namespace Constellation.Core.Refactor.Exceptions;

using System;

public class UnsupportedResourceTypeException : Exception
{
    public UnsupportedResourceTypeException(string code)
        : base($"Class Resource Type \"{code}\" is unsupported.")
    {
    }
}
