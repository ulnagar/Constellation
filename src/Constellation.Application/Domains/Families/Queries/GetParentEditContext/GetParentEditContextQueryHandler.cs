namespace Constellation.Application.Domains.Families.Queries.GetParentEditContext;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using Core.Models.Families.Errors;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetParentEditContextQueryHandler
    : IQueryHandler<GetParentEditContextQuery, ParentEditContextResponse>
{
    private readonly IFamilyRepository _familyRepository;
    
    public GetParentEditContextQueryHandler(
        IFamilyRepository familyRepository)
    {
        _familyRepository = familyRepository;
    }

    public async Task<Result<ParentEditContextResponse>> Handle(GetParentEditContextQuery request, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
            return Result.Failure<ParentEditContextResponse>(FamilyErrors.NotFound(request.FamilyId));

        var parent = family.Parents.FirstOrDefault(parent => parent.Id == request.ParentId);

        if (parent is null)
            return Result.Failure<ParentEditContextResponse>(ParentErrors.NotFoundInFamily(request.ParentId, request.FamilyId));

        var entry = new ParentEditContextResponse(
            parent.Id,
            family.Id,
            parent.Title,
            parent.FirstName,
            parent.LastName,
            parent.MobileNumber,
            parent.EmailAddress);

        return entry;
    }
}
