using System.Collections.Generic;

namespace Constellation.Application.DTOs.EmailRequests
{
    public abstract class EmailBaseClass
    {
        public EmailBaseClass()
        {
            Recipients = new List<Recipient>();
        }

        public ICollection<Recipient> Recipients { get; set; }

        public class Recipient
        {
            public string Name { get; set; }
            public string Email { get; set; }
        }
    }
}
