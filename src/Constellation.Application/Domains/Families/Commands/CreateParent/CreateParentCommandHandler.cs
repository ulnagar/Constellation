namespace Constellation.Application.Domains.Families.Commands.CreateParent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Shared;
using Core.Models.Families.Errors;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateParentCommandHandler
    : ICommandHandler<CreateParentCommand, Parent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateParentCommandHandler(
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork)
    {
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Parent>> Handle(CreateParentCommand request, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
            return Result.Failure<Parent>(FamilyErrors.NotFound(request.FamilyId));

        var result = family.AddParent(
            request.Title,
            request.FirstName,
            request.LastName,
            request.MobileNumber,
            request.EmailAddress,
            Parent.SentralReference.None);

        if (result.IsSuccess)
            await _unitOfWork.CompleteAsync(cancellationToken);

        return result;
    }
}
