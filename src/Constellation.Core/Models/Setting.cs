namespace Constellation.Core.Models
{
    public class Setting
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public Setting(string name, string type, string value)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(type))
            {
                return;
            }

            Name = name;
            Type = type;
            Value = value;
        }

        public void Modify(string value)
        {
            Value = value;
        }

        public static class SettingName
        {
            public const string Active = "Active";
            public const string CancelSessions = "CancelSessions";
            public const string CCList = "CCList";
            public const string FolderSCO = "FolderSCO";
            public const string Key = "Key";
            public const string Password = "Password";
            public const string Port = "Port";
            public const string Server = "Server";
            public const string ServerURL = "ServerURL";
            public const string SiteName = "SiteName";
            public const string Token = "Token";
            public const string Username = "Username";
            public const string Preference = "Preference";
        }

        public static class SettingType
        {
            public const string AdobeConnect = "AdobeConnect";
            public const string Application = "Application";
            public const string DataTask = "DataTask";
            public const string EmailSender = "EmailSender";
            public const string MSTeams = "MSTeams";
            public const string BUD = "BUD";
            public const string SMSGateway = "SMSGateway";
            public const string Sentral = "Sentral";
            public const string Firebase = "Firebase";
        }
    }

}
