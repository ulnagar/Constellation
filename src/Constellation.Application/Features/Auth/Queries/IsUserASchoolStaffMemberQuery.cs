using MediatR;

namespace Constellation.Application.Features.Auth.Queries
{
    public class IsUserASchoolStaffMemberQuery : IRequest<bool>
    {
        public string EmailAddress { get; set; }
    }
}
