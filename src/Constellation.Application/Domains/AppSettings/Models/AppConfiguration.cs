namespace Constellation.Application.Domains.AppSettings.Models;

using Core.Models.Absences.Enums;

public sealed class AppConfiguration
{
    public const string Section = "Constellation:AppSettings";

    public string DebugLabel { get; set; }
    public string AdminUser { get; set; } 
    public AttachmentsConfiguration Attachments { get; set; }

    public class AttachmentsConfiguration
    {
        public string BaseFilePath { get; set; }
        public int MaxDBStoreSize { get; set; }
    }
}
