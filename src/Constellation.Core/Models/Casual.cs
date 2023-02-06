using System;
using System.Collections.Generic;
using Constellation.Core.Models.Covers;

namespace Constellation.Core.Models
{
    public class Casual
    {
        public Casual()
        {
            IsDeleted = false;
            DateEntered = DateTime.Now;

            AdobeConnectOperations = new List<CasualAdobeConnectOperation>();
            MSTeamOperations = new List<CasualMSTeamOperation>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PortalUsername { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateDeleted { get; set; }
        public DateTime? DateEntered { get; set; }
        public string AdobeConnectPrincipalId { get; set; }
        public string SchoolCode { get; set; }
        public School School { get; set; }
        public string EmailAddress => PortalUsername + "@det.nsw.edu.au";
        public string DisplayName => FirstName + " " + LastName;
        public ICollection<CasualAdobeConnectOperation> AdobeConnectOperations { get; set; }
        public ICollection<CasualMSTeamOperation> MSTeamOperations { get; set; }
    }
}