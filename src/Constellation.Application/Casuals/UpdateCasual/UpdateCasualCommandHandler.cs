using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Casuals.UpdateCasual;
internal sealed class UpdateCasualCommandHandler
    : ICommandHandler<UpdateCasualCommand>
{
    private readonly ICasualRepository _casualRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCasualCommandHandler(
        ICasualRepository casualRepository,
        IUnitOfWork unitOfWork)
    {
        _casualRepository = casualRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateCasualCommand request, CancellationToken cancellationToken)
    {
        var casual = await _casualRepository.GetById(request.Id, cancellationToken);

        if (casual is null)
        {
            return Result.Failure(DomainErrors.Casuals.Casual.NotFound(request.Id));
        }

        var nameResult = Name.Create(request.FirstName, string.Empty, request.LastName);

        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        casual.Update(
            nameResult.Value,
            request.AdobeConnectId,
            request.SchoolCode);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
