using MediatR;

namespace Constellation.Application.Features.Auth.Command
{
    public class RegisterADUserAsSchoolContactCommand : IRequest
    {
        public string EmailAddress { get; set; }
    }
}
