namespace Constellation.Core.Refactor.Exceptions;

using System;

public class UnsupportedTeacherTypeException : Exception
{
    public UnsupportedTeacherTypeException(string code)
        : base($"Teacher Type \"{code}\" is unsupported.")
    {
    }
}
