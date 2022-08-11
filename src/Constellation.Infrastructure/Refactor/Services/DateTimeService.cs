namespace Constellation.Infrastructure.Refactor.Services;

using Constellation.Application.Refactor.Common.Interfaces;
using System;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
}
