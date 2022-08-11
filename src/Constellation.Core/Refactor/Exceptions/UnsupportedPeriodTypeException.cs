namespace Constellation.Core.Refactor.Exceptions;

using System;

public class UnsupportedPeriodTypeException : Exception
{
    public UnsupportedPeriodTypeException(string code)
        : base($"Period Type \"{code}\" is unsupported.")
    {
    }
}
