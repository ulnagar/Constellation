namespace Constellation.Core.Comparators;

using Models.StaffMembers;
using System;
using System.Collections.Generic;
using ValueObjects;

public class StaffComparator : IEqualityComparer<StaffMember>
{
    public bool Equals(StaffMember? x, StaffMember? y)
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

    public int GetHashCode(StaffMember obj)
    {
        //Get hash code for the room field if it is not null.
        int hashProductName = 
            obj.EmailAddress.Equals(EmailAddress.None)
                ? 0 
                : obj.EmailAddress.Email.GetHashCode(StringComparison.InvariantCultureIgnoreCase);

        //Calculate the hash code for the room.
        return hashProductName;
    }
}