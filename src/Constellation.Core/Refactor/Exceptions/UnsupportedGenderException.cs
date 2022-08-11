namespace Constellation.Core.Refactor.Exceptions;

using System;

public class UnsupportedGenderException : Exception
{
    public UnsupportedGenderException(string code)
        : base($"Gender \"{code}\" is unsupported.")
    {
    }
}
