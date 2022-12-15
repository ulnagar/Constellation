namespace Constellation.Core.Models;

public class StudentFamily
{
    public string Id { get; set; } = string.Empty;
    public List<Student> Students { get; set; } = new();
    public Parent Parent1 { get; set; } = new();
    public Parent Parent2 { get; set; } = new();
    public MailingAddress Address { get; set; } = new();

    public class Parent
    {
        public string Title { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty; 
        public string EmailAddress { get; set; } = string.Empty;
    }

    public class MailingAddress
    {
        public string Title { get; set; } = string.Empty;
        public string Line1 { get; set; } = string.Empty;
        public string Line2 { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public string PostCode { get; set; } = string.Empty;
    }
}
