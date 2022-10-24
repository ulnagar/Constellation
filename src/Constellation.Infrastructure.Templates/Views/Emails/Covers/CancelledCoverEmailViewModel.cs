using Constellation.Infrastructure.Templates.Views.Shared;
using System;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Templates.Views.Emails.Covers
{
    public class CancelledCoverEmailViewModel : EmailLayoutBaseViewModel
    {
        public CancelledCoverEmailViewModel()
        {
            ClassWithLink = new Dictionary<string, string>();
        }

        public string ToName { get; set; }
        public IDictionary<string, string> ClassWithLink { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool HasAdobeAccount { get; set; }

        public string DateBlock => buildDateBlock();
        private string buildDateBlock()
        {
            var returnString = "";

            if (StartDate.Date != EndDate.Date)
            {
                returnString += $"from {StartDate.ToShortDateString()} until {EndDate.ToShortDateString()} ";
            } else
            {
                returnString += $"on {StartDate.ToShortDateString()} ";
            }

            if (ClassWithLink.Count > 1)
            {
                returnString += $"for the classes listed below:";
            } else
            {
                returnString += $"for the class listed below:";
            }

            return returnString;
        }

    }
}
