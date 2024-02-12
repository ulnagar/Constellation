namespace Constellation.Application.Families.CreateFamily;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateFamilyCommandHandler
    : ICommandHandler<CreateFamilyCommand, Family>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFamilyCommandHandler(
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork)
    {
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Family>> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
    {
        bool existingFamily = await _familyRepository.DoesFamilyWithEmailExist(request.FamilyEmail, cancellationToken);

        if (existingFamily)
        {
            // Check if existing family is deleted, and reactivate
            Family family = await _familyRepository.GetFamilyByEmail(request.FamilyEmail, cancellationToken);

            if (!family.IsDeleted) 
                return Result.Failure<Family>(DomainErrors.Families.Family.EmailAlreadyInUse);

            family.Reinstate();
            await _unitOfWork.CompleteAsync(cancellationToken);
            
            return family;
        }

        Family newFamily = Family.Create(new FamilyId(), request.FamilyTitle);

        Result result = newFamily.UpdateFamilyAddress(
            request.FamilyTitle,
            request.AddressLine1,
            request.AddressLine2 ?? string.Empty,
            request.AddressTown,
            request.AddressPostCode);

        if (result.IsFailure)
            return Result.Failure<Family>(result.Error);

        Result emailResult = newFamily.UpdateFamilyEmail(request.FamilyEmail.Email);

        if (emailResult.IsFailure)
            return Result.Failure<Family>(emailResult.Error);

        _familyRepository.Insert(newFamily);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return newFamily;
    }
}
