﻿namespace Constellation.Application.Families.UpdateParent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Families;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateParentCommandHandler
    : ICommandHandler<UpdateParentCommand, Parent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateParentCommandHandler(
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork)
    {
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Parent>> Handle(UpdateParentCommand request, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
            return Result.Failure<Parent>(DomainErrors.Families.Family.NotFound(request.FamilyId));

        var parent = family.Parents.FirstOrDefault(parent => parent.Id == request.ParentId);

        if (parent is null)
            return Result.Failure<Parent>(DomainErrors.Families.Parents.NotFoundInFamily(request.ParentId, request.FamilyId));

        var result = family.UpdateParent(
            request.ParentId,
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
