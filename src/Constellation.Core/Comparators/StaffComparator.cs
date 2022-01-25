using Constellation.Core.Models;
using System.Collections.Generic;

namespace Constellation.Core.Comparators
{
    public class StaffComparator : IEqualityComparer<Staff>
    {
        public bool Equals(Staff x, Staff y)
        {
            // Check whether the compared objects reference the same data
            if (object.ReferenceEquals(x, y))
                return true;

            // Check whether any of the compared objects is null
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
                return false;

            // Check whether the objects properties are equal
            return x.EmailAddress == y.EmailAddress;
        }

        public int GetHashCode(Staff obj)
        {
            //Get hash code for the room field if it is not null.
            var hashProductName = obj.EmailAddress == null ? 0 : obj.EmailAddress.GetHashCode();

            //Calculate the hash code for the room.
            return hashProductName;
        }
    }
}
