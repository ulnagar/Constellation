namespace Constellation.Core.Comparators;

using Models;
using System;
using System.Collections.Generic;

public class StaffComparator : IEqualityComparer<Staff>
{
    public bool Equals(Staff? x, Staff? y)
    {
        // Check whether the compared objects reference the same data
        if (ReferenceEquals(x, y))
            return true;

        // Check whether any of the compared objects is null
        if (x is null || y is null)
            return false;

        // Check whether the objects properties are equal
        return x.EmailAddress == y.EmailAddress;
    }

    public int GetHashCode(Staff obj)
    {
        //Get hash code for the room field if it is not null.
        int hashProductName = 
            string.IsNullOrWhiteSpace(obj.EmailAddress) 
                ? 0 
                : obj.EmailAddress.GetHashCode(StringComparison.InvariantCultureIgnoreCase);

        //Calculate the hash code for the room.
        return hashProductName;
    }
}