using Constellation.Core.Models.Subjects;
using System;
using System.Collections.Generic;

namespace Constellation.Core.Models
{
    public class AdobeConnectRoom
    {
        public AdobeConnectRoom()
        {
            OfferingSessions = new List<Session>();
        }

        public string ScoId { get; set; }
        public string Name { get; set; }
        public string UrlPath { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateDeleted { get; set; }
        public bool Protected { get; set; }
        public ICollection<Session> OfferingSessions { get; set; }
    }
}