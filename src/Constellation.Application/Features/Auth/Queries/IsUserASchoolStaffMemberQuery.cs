namespace Constellation.Application.Features.Auth.Queries;

using MediatR;

public sealed class IsUserASchoolStaffMemberQuery : IRequest<bool>
{
    public string EmailAddress { get; set; }
}