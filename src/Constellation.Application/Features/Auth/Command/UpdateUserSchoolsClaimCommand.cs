using MediatR;

namespace Constellation.Application.Features.Auth.Command
{
    public class UpdateUserSchoolsClaimCommand : IRequest
    {
        public string EmailAddress { get; set; }
    }
}
