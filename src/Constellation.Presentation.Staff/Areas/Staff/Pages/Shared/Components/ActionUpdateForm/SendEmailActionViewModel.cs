namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ActionUpdateForm;

using Microsoft.AspNetCore.Http;

public sealed class SendEmailActionViewModel
{
    public Dictionary<string, string> Recipients { get; set; } = new();

    public string SenderName { get; set; }
    public string SenderEmail { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = "<p><br></p><p>Regards,</p><p><br></p>\r\n<p>Aurora College</p>\r\n<p>T: 1300 287 629</p>\r\n<p>W: www.aurora.nsw.edu.au</p>";

    public DateTime SentAt { get; set; }
    public List<Contact> Contacts { get; set; } = new();
    public List<IFormFile> Attachments { get; set; } = new();
    public List<Contact> Senders { get; set; } = new();

    public sealed class Contact
    {
        public ContactType Type { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }
    }

    public enum ContactType
    {
        Student,
        ResidentialFamily,
        NonResidentialFamily,
        PartnerSchool,
        AuroraCollege
    }
}
