using System;

namespace Constellation.Infrastructure.Persistence.TrackIt.Models
{
    public partial class Index
    {
        public DateTime Lastmodified { get; set; }
        public string Name { get; set; }
        public int Recnum { get; set; }
    }
}
