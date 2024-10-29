namespace Constellation.Application.Interfaces.Configuration;

using MimeKit.Encodings;
using System.Collections.Generic;

public class SentralGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:Sentral";

    public string Username { get; set; }
    public string Password { get; set; }
    public string ServerUrl { get; set; }

    public string ApiKey { get; set; }
    public string ApiTenant { get; set; }

    public ContactPreferenceOptions ContactPreference { get; set; }
    public SentralXPathLocations XPaths { get; set; }

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(ServerUrl))
            return false;

        if (string.IsNullOrWhiteSpace(Username))
            return false;

        if (string.IsNullOrWhiteSpace(Password))
            return false;

        return true;
    }

    public class SentralXPathLocations
    {
        public string FamilyEmail { get; set; }
        public string Parent1Name { get; set; }
        public string Parent1Mobile { get; set; }
        public string Parent1Email { get; set; }
        public string Parent2Name { get; set; }
        public string Parent2Mobile { get; set; }
        public string Parent2Email { get; set; }
        public string FamilyName { get; set; }
        public string AbsenceTable { get; set; }
        public string StudentTable { get; set; }
        public string PartialAbsenceTable { get; set; }
        public string CalendarTable { get; set; }
        public string TermCalendarTable { get; set; }
        public string WellbeingStudentAwardsList { get; set; }
        public string IncidentCreatedDate { get; set; }
    }

    public enum ContactPreferenceOptions
    {
        MotherThenFather,
        FatherThenMother,
        Both
    }
}
