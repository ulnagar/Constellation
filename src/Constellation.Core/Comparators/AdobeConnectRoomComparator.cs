using Constellation.Core.Models;
using System.Collections.Generic;

namespace Constellation.Core.Comparators
{
    public class AdobeConnectRoomComparator : IEqualityComparer<AdobeConnectRoom>
    {
        public bool Equals(AdobeConnectRoom x, AdobeConnectRoom y)
        {
            // Check whether the compared objects reference the same data
            if (object.ReferenceEquals(x, y))
                return true;

            // Check whether any of the compared objects is null
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
                return false;

            // Check whether the objects properties are equal
            return x.ScoId == y.ScoId;
        }

        public int GetHashCode(AdobeConnectRoom room)
        {
            //Get hash code for the room field if it is not null.
            var hashProductName = room.ScoId == null ? 0 : room.ScoId.GetHashCode();

            //Calculate the hash code for the room.
            return hashProductName;
        }
    }
}
