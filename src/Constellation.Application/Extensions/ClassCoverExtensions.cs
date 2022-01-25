using Constellation.Core.Models;
using System;

namespace Constellation.Application.Extensions
{
    public static class ClassCoverExtensions
    {
        public static bool IsCurrent(this ClassCover cover)
        {
            if (cover.IsDeleted == true)
                return false;

            if (cover.EndDate <= DateTime.Today)
                return false;

            if (cover.StartDate >= DateTime.Today)
                return false;

            return true;
        }

        public static bool IsFuture(this ClassCover cover)
        {
            if (cover.StartDate.Date > DateTime.Today && !cover.IsDeleted)
                return true;

            return false;
        }
    }
}
