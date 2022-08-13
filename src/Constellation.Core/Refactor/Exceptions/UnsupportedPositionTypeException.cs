namespace Constellation.Core.Refactor.Exceptions;

using System;

public class UnsupportedPositionTypeException : Exception
{
    public UnsupportedPositionTypeException(string code)
        : base($"Position Type \"{code}\" is unsupported.")
    {
    }
}
