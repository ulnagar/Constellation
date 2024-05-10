namespace Constellation.Application.Interfaces.Configuration;

using System.Collections.Generic;

public class SentralGatewayConfiguration
{
    public const string Section = "Constellation:Gateways:Sentral";

    public string Username { get; set; }
    public string Password { get; set; }
    public string ServerUrl { get; set; }
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
        public string MothersHomePhone { get; set; }
        public string MothersWorkPhone { get; set; }
        public string MothersMobilePhone { get; set; }
        public string MothersEmail { get; set; }
        public string FathersHomePhone { get; set; }
        public string FathersWorkPhone { get; set; }
        public string FathersMobilePhone { get; set; }
        public string FathersEmail { get; set; }
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
