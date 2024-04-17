namespace Constellation.Presentation.Server.Pages.Shared.Components.ActionUpdateForm;

public sealed class SendEmailActionViewModel
{
    public Dictionary<string, string> Recipients { get; set; } = new();

    public string SenderName { get; set; }
    public string SenderEmail { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

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
