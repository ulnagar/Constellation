using MediatR;

namespace Constellation.Application.Features.Auth.Command
{
    public class RepairSchoolContactUserCommand : IRequest
    {
        public int SchoolContactId { get; set; }
    }
}
