using System.Collections.Generic;

namespace Constellation.Application.DTOs
{
    public class SMSMessageCollectionDto
    {
        public SMSMessageCollectionDto()
        {
            Messages = new List<Message>();
        }
        
        public ICollection<Message> Messages { get; set; }

        public class Message
        {
            public string Id { get; set; }
            public string OutgoingId { get; set; }
            public string Origin { get; set; }
            public string MessageBody { get; set; }
            public string Timestamp { get; set; }
            public string Status { get; set; }
            public string Destination { get; set; }
        }
    }
}
