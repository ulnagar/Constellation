namespace Constellation.Application.Features.Auth.Queries;

using Constellation.Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolContactByEmailAddressQueryHandler 
    : IRequestHandler<GetSchoolContactByEmailAddressQuery, SchoolContact>
{
    private readonly ISchoolContactRepository _contactRepository;

    public GetSchoolContactByEmailAddressQueryHandler(
        ISchoolContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<SchoolContact> Handle(GetSchoolContactByEmailAddressQuery request, CancellationToken cancellationToken) => 
        await _contactRepository.GetWithRolesByEmailAddress(request.EmailAddress, cancellationToken);
}