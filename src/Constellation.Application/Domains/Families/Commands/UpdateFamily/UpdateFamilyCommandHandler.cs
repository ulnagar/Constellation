namespace Constellation.Application.Domains.Families.Commands.UpdateFamily;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using Core.Models.Families.Errors;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateFamilyCommandHandler
    : ICommandHandler<UpdateFamilyCommand>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFamilyCommandHandler(
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork)
    {
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
            return Result.Failure(FamilyErrors.NotFound(request.FamilyId));

        var addressUpdate = family.UpdateFamilyAddress(
            request.FamilyTitle,
            request.AddressLine1,
            string.IsNullOrWhiteSpace(request.AddressLine2) ? string.Empty : request.AddressLine2,
            request.AddressTown,
            request.AddressPostCode);

        if (addressUpdate.IsFailure)
            return addressUpdate;

        if (request.FamilyEmail.ToLower() != family.FamilyEmail.ToLower())
        {
            var emailUpdate = family.UpdateFamilyEmail(request.FamilyEmail);

            if (emailUpdate.IsFailure)
                return emailUpdate;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
