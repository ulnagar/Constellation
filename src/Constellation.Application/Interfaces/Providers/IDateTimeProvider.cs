namespace Constellation.Application.Interfaces.Providers;

using System;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime Today { get; }
}
