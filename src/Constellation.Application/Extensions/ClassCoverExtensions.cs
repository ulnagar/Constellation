namespace Constellation.Application.Extensions;

using Constellation.Core.Models.Covers;
using System;

public static class ClassCoverExtensions
{
    public static bool IsCurrent(this ClassCover cover)
    {
        if (cover.IsDeleted == true)
            return false;

        if (cover.EndDate < DateOnly.FromDateTime(DateTime.Today))
            return false;

        if (cover.StartDate > DateOnly.FromDateTime(DateTime.Today))
            return false;

        return true;
    }

    public static bool IsFuture(this ClassCover cover)
    {
        if (cover.StartDate > DateOnly.FromDateTime(DateTime.Today) && !cover.IsDeleted)
            return true;

        return false;
    }
}
