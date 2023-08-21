using Constellation.Core.Models.Subjects;
using System;
using System.Linq;

namespace Constellation.Application.Extensions
{
    public static class CourseOfferingExtensions
    {
        /// <summary>
        /// Must have the Sessions collection loaded to get accurate information
        /// </summary>
        /// <param name="offering"></param>
        /// <returns>bool</returns>
        public static bool IsCurrent(this Offering offering)
        {
            if (offering.Sessions.All(s => s.IsDeleted))
                return false;

            if (offering.StartDate <= DateTime.Today && offering.EndDate >= DateTime.Today)
                return true;

            return false;
        }
    }
}
