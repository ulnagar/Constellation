using MediatR;

namespace Constellation.Application.Features.Auth.Queries
{
    public class IsUserAParentQuery : IRequest<bool>
    {
        public string EmailAddress { get; set; }
    }
}
