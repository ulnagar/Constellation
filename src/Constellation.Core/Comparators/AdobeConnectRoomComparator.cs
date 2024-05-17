namespace Constellation.Core.Comparators;

using Models;
using System;
using System.Collections.Generic;

public class AdobeConnectRoomComparator : IEqualityComparer<AdobeConnectRoom>
{
    public bool Equals(AdobeConnectRoom? x, AdobeConnectRoom? y)
    {
        // Check whether the compared objects reference the same data
        if (ReferenceEquals(x, y))
            return true;

        // Check whether any of the compared objects is null
        if (x is null || y is null)
            return false;

        // Check whether the objects properties are equal
        return x.ScoId == y.ScoId;
    }

    public int GetHashCode(AdobeConnectRoom? obj)
    {
        //Get hash code for the room field if it is not null.
        int hashProductName = obj?.ScoId == null ? 0 : obj.ScoId.GetHashCode(StringComparison.Ordinal);

        //Calculate the hash code for the room.
        return hashProductName;
    }
}