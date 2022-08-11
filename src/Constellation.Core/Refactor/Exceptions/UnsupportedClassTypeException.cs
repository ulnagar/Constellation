namespace Constellation.Core.Refactor.Exceptions;

using System;

public class UnsupportedClassTypeException : Exception
{
    public UnsupportedClassTypeException(string code)
        : base($"Class Type \"{code}\" is unsupported.")
    {
    }
}
