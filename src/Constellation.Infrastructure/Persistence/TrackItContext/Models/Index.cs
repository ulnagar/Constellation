using System;

namespace Constellation.Infrastructure.Persistence.TrackItContext.Models
{
    public partial class Index
    {
        public DateTime Lastmodified { get; set; }
        public string Name { get; set; }
        public int Recnum { get; set; }
    }
}
