using System;
namespace Constellation.Application.Interfaces.Providers;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now { get => DateTime.Now; }
    public DateTime Today { get => DateTime.Today; }
}
