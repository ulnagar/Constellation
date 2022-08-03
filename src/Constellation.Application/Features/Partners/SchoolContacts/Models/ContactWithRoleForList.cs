namespace Constellation.Application.Features.Partners.SchoolContacts.Models
{
    public class ContactWithRoleForList
    {
        public int ContactId { get; set; }
        public int AssignmentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Position { get; set; }
    }
}
