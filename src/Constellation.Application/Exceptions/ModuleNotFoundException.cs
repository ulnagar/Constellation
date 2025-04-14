namespace Constellation.Application.Exceptions;

using System;

public sealed class ModuleNotFoundException(string message) 
    : Exception(message);