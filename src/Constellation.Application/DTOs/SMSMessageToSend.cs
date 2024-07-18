using System.Collections.Generic;

namespace Constellation.Application.DTOs
{
    public class SMSMessageToSend
    {
        public string origin { get; set; }
        public string message { get; set; }
        public ICollection<string> destinations { get; set; }
        //public string notifyUrl { get; set; }

        public SMSMessageToSend()
        {
            destinations = new List<string>();
        }
    }
}
