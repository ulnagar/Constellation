﻿namespace Constellation.Application.Domains.Families.Commands.DeleteParentById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Shared;
using Core.Models.Families.Errors;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DeleteParentByIdCommandHandler
    : ICommandHandler<DeleteParentByIdCommand>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteParentByIdCommandHandler(
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork)
    {
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteParentByIdCommand request, CancellationToken cancellationToken)
    {
        Family family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
        {
            return Result.Failure(FamilyErrors.NotFound(request.FamilyId));
        }

        Parent parent = family.Parents.FirstOrDefault(parent => parent.Id == request.ParentId);

        if (parent is null)
        {
            return Result.Failure(ParentErrors.NotFoundInFamily(request.ParentId, request.FamilyId));
        }

        Result result = family.RemoveParent(parent.Id);

        if (result.IsSuccess)
            await _unitOfWork.CompleteAsync(cancellationToken);
        
        return result;
    }
}
