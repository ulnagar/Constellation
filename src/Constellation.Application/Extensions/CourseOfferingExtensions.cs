using Constellation.Core.Models;
using System;
using System.Linq;

namespace Constellation.Application.Extensions
{
    public static class CourseOfferingExtensions
    {
        public static bool IsCurrent(this CourseOffering offering)
        {
            if (offering.Sessions.All(s => s.IsDeleted))
                return false;

            if (offering.StartDate <= DateTime.Today && offering.EndDate >= DateTime.Today)
                return true;

            return false;
        }
    }
}
