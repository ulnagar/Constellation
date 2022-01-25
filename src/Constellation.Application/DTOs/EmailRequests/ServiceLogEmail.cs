using System.Collections.Generic;

namespace Constellation.Application.DTOs.EmailRequests
{
    public class ServiceLogEmail : EmailBaseClass
    {
        public ServiceLogEmail()
        {
            Log = new List<string>();
        }

        public ICollection<string> Log { get; set; }
        public string Source { get; set; }
    }
}
