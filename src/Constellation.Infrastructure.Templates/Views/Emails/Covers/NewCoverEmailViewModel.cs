namespace Constellation.Infrastructure.Templates.Views.Emails.Covers;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;
using System.Collections.Generic;

public sealed class NewCoverEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Covers/NewCoverEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    public string Alert => BuildAlertString();

    public required string ContactName { get; set; }
    public required string ContactPhone { get; set; } = "1300 287 629"; 
        
    public required string ToName { get; set; }
    public Dictionary<string, string> ClassWithLink { get; set; } = [];
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public string DateBlock => BuildDateBlock();

    private string BuildAlertString()
    {
        if (string.IsNullOrWhiteSpace(ContactName))
        {
            return "Please note that declining this calendar appointment does not cancel the cover. If you are unable to teach this class, you must contact Aurora College as soon as possible.";
        }

        return $"Please note that declining this calendar appointment does not cancel the cover. If you are unable to teach this class, you must contact {ContactName} on {ContactPhone} as soon as possible.";
    }

    private string BuildDateBlock()
    {
        string returnString = "";

        if (StartDate.Date != EndDate.Date)
        {
            returnString += $"from {StartDate.ToShortDateString()} until {EndDate.ToShortDateString()} ";
        }
        else
        {
            returnString += $"on {StartDate.ToShortDateString()} ";
        }

        if (ClassWithLink.Count > 1)
        {
            returnString += $"for the classes listed below:";
        }
        else
        {
            returnString += $"for the class listed below:";
        }

        return returnString;
    }
}