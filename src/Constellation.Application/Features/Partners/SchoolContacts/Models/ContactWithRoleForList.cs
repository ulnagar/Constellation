using Constellation.Core.Models;

namespace Constellation.Application.Features.Partners.SchoolContacts.Models
{
    public class ContactWithRoleForList
    {
        public int ContactId { get; set; }
        public int AssignmentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Position { get; set; }
        public int PositionSort()
        {
            switch (Position)
            {
                case SchoolContactRole.Principal:
                    return 1;
                case SchoolContactRole.Coordinator:
                    return 2;
                case SchoolContactRole.SciencePrac:
                    return 3;
                default:
                    return 10;
            }
        }
    }
}
