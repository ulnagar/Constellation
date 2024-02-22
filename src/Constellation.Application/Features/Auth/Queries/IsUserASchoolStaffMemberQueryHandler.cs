namespace Constellation.Application.Features.Auth.Queries;

using Constellation.Application.Interfaces.Repositories;
using Core.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

internal sealed class IsUserASchoolStaffMemberQueryHandler 
    : IRequestHandler<IsUserASchoolStaffMemberQuery, bool>
{
    private readonly IStaffRepository _staffRepository;


    public IsUserASchoolStaffMemberQueryHandler(
        IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<bool> Handle(IsUserASchoolStaffMemberQuery request, CancellationToken cancellationToken)
    {
        Staff staffMember = await _staffRepository.GetCurrentByEmailAddress(request.EmailAddress, cancellationToken);

        return staffMember is not null;
    }
}