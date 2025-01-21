namespace Constellation.Application.Features.API.Operations.Commands;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models.Canvas.Models;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Shared;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ProcessCanvasOperationCommandHandler 
    : ICommandHandler<ProcessCanvasOperationCommand>
{
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly ICanvasGateway _canvasGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ProcessCanvasOperationCommandHandler(
        ICanvasOperationsRepository operationsRepository,
        ICanvasGateway canvasGateway,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _operationsRepository = operationsRepository;
        _canvasGateway = canvasGateway;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ProcessCanvasOperationCommand>();
    }
    
    // TODO: R1.16.0: Remove operations entirely.
    // Move account management features to real-time handlers.
    // Rely on audit for per course enrolment management.
    // This would allow caching of Canvas User Id with principals (e.g. Student SystemLinks)
    public async Task<Result> Handle(ProcessCanvasOperationCommand request, CancellationToken cancellationToken)
    {
        CanvasOperation operation = await _operationsRepository.WithDetails(request.OperationId, cancellationToken);

        if (operation is null)
        {
            _logger
                .ForContext(nameof(ProcessCanvasOperationCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Operations.Canvas.NotFound(request.OperationId), true)
                .Warning("Failed to process Canvas Operation");

            return Result.Failure(DomainErrors.Operations.Canvas.NotFound(request.OperationId));
        }
        
        switch (operation.GetType().Name)
        {
            case nameof(CreateUserCanvasOperation):
                CreateUserCanvasOperation createOperation = operation as CreateUserCanvasOperation;

                _logger
                    .ForContext(nameof(CreateUserCanvasOperation), createOperation, true)
                    .Information("Creating Canvas user for {name}", $"{createOperation!.FirstName} {createOperation.LastName}");

                Result createSuccess = await _canvasGateway.CreateUser(
                    createOperation.UserId, 
                    createOperation.FirstName, 
                    createOperation.LastName, 
                    $"{createOperation.PortalUsername}@education.nsw.gov.au", 
                    createOperation.EmailAddress, 
                    cancellationToken);

                if (createSuccess.IsSuccess)
                {
                    _logger
                        .ForContext(nameof(CreateUserCanvasOperation), createOperation, true)
                        .Information("Successfully created Canvas user for {name}", $"{createOperation.FirstName} {createOperation.LastName}");

                    createOperation.Complete();

                    await _unitOfWork.CompleteAsync(cancellationToken);

                    return Result.Success();
                }

                _logger
                    .ForContext(nameof(CreateUserCanvasOperation), createOperation, true)
                    .Warning("Failed to create Canvas user for {name}", $"{createOperation.FirstName} {createOperation.LastName}");

                return Result.Failure(DomainErrors.Operations.Canvas.ProcessFailed);

            case nameof(UpdateUserEmailCanvasOperation):
                UpdateUserEmailCanvasOperation updateOperation = operation as UpdateUserEmailCanvasOperation;

                _logger
                    .ForContext(nameof(UpdateUserEmailCanvasOperation), updateOperation, true)
                    .Information("Updating Canvas user {UserId} to new Email Address", updateOperation!.UserId);

                Result updateSuccess = await _canvasGateway.UpdateUserEmail(updateOperation.UserId, $"{updateOperation.PortalUsername}@education.nsw.gov.au", cancellationToken);

                if (updateSuccess.IsSuccess)
                {
                    _logger
                        .ForContext(nameof(UpdateUserEmailCanvasOperation), updateOperation, true)
                        .Information("Successfully updated Canvas user {UserId} to new Email Address", updateOperation.UserId);

                    updateOperation.Complete();

                    await _unitOfWork.CompleteAsync(cancellationToken);

                    return Result.Success();
                }

                _logger
                    .ForContext(nameof(UpdateUserEmailCanvasOperation), updateOperation, true)
                    .ForContext(nameof(Error), updateSuccess.Error, true)
                    .Warning("Failed to update Canvas user {UserId} to new Email Address", updateOperation.UserId);

                return Result.Failure(updateSuccess.Error);

            case nameof(ModifyEnrolmentCanvasOperation):
                ModifyEnrolmentCanvasOperation modifyOperation = operation as ModifyEnrolmentCanvasOperation;

                _logger
                    .ForContext(nameof(ModifyEnrolmentCanvasOperation), modifyOperation, true)
                    .Information("Modifying Canvas enrollment for {name}", $"{modifyOperation!.UserId}");

                Result modifySuccess = modifyOperation.Action switch
                {
                    _ when modifyOperation.Action.Equals(CanvasAction.Add) && modifyOperation.UserType.Equals(CanvasUserType.Teacher) =>
                        await _canvasGateway.EnrolToCourse(modifyOperation.UserId, modifyOperation.CourseId, CanvasPermissionLevel.Teacher, cancellationToken),
                    _ when modifyOperation.Action.Equals(CanvasAction.Add) && modifyOperation.UserType.Equals(CanvasUserType.Student) && request.UseSections && modifyOperation.SectionId != CanvasSectionCode.Empty =>
                        await _canvasGateway.EnrolToSection(modifyOperation.UserId, modifyOperation.CourseId, modifyOperation.SectionId, CanvasPermissionLevel.Student, cancellationToken),
                    _ when modifyOperation.Action.Equals(CanvasAction.Add) && modifyOperation.UserType.Equals(CanvasUserType.Student) =>
                        await _canvasGateway.EnrolToCourse(modifyOperation.UserId, modifyOperation.CourseId, CanvasPermissionLevel.Student, cancellationToken),
                    _ when modifyOperation.Action.Equals(CanvasAction.Remove) =>
                        await _canvasGateway.UnenrolUser(modifyOperation.UserId, modifyOperation.CourseId, cancellationToken),
                    _ => throw new NotImplementedException()
                };
                
                if (modifySuccess.IsSuccess)
                {
                    _logger
                        .ForContext(nameof(ModifyEnrolmentCanvasOperation), modifyOperation, true)
                        .Information("Successfully modified Canvas enrollment for {name}", $"{modifyOperation.UserId}");

                    modifyOperation.Complete();

                    await _unitOfWork.CompleteAsync(cancellationToken);

                    return Result.Success();
                }

                _logger
                    .ForContext(nameof(ModifyEnrolmentCanvasOperation), modifyOperation, true)
                    .ForContext(nameof(Error), modifySuccess.Error, true)
                    .Warning("Failed to modify Canvas enrollment for {name}", $"{modifyOperation.UserId}");

                return Result.Failure(modifySuccess.Error);

            case nameof(DeleteUserCanvasOperation):
                DeleteUserCanvasOperation deleteOperation = operation as DeleteUserCanvasOperation;

                _logger
                    .ForContext(nameof(DeleteUserCanvasOperation), deleteOperation, true)
                    .Information("Deleting Canvas user for {name}", $"{deleteOperation!.UserId}");

                Result deleteSuccess = await _canvasGateway.DeactivateUser(deleteOperation.UserId, cancellationToken);

                if (deleteSuccess.IsSuccess)
                {
                    _logger
                        .ForContext(nameof(DeleteUserCanvasOperation), deleteOperation, true)
                        .Information("Successfully deleted Canvas user for {name}", $"{deleteOperation.UserId}");

                    deleteOperation.Complete();

                    await _unitOfWork.CompleteAsync(cancellationToken);

                    return Result.Success();
                }

                _logger
                    .ForContext(nameof(DeleteUserCanvasOperation), deleteOperation, true)
                    .ForContext(nameof(Error), deleteSuccess.Error, true)
                    .Warning("Failed to delete Canvas user for {name}", $"{deleteOperation.UserId}");

                return Result.Failure(deleteSuccess.Error);
        }

        return Result.Failure(DomainErrors.Operations.Canvas.Invalid);
    }
}