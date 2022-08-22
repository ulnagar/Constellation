using MediatR;

namespace Constellation.Application.Features.Partners.SchoolContacts.Commands
{
    public class CreateNewAssignmentForSchoolContactCommand : IRequest
    {
        public int ContactId { get; set; }
        public string SchoolCode { get; set; }
        public string Position { get; set; }
    }
}
