namespace Constellation.Application.Extensions;

using Constellation.Application.Interfaces.Providers;
using Constellation.Core.Models;

public static class ClassCoverExtensions
{
    public static bool IsCurrent(this ClassCover cover, IDateTimeProvider dateTimeProvider)
    {
        if (cover.IsDeleted == true)
            return false;

        if (cover.EndDate <= dateTimeProvider.Today)
            return false;

        if (cover.StartDate >= dateTimeProvider.Today)
            return false;

        return true;
    }

    public static bool IsFuture(this ClassCover cover, IDateTimeProvider dateTimeProvider)
    {
        if (cover.StartDate.Date > dateTimeProvider.Today && !cover.IsDeleted)
            return true;

        return false;
    }
}
