namespace Constellation.Application.Features.API.Operations.Commands;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models.Canvas.Models;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Shared;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Microsoft.Extensions.Options;
using Serilog;
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
                    .Information("Creating Canvas user for {name}", $"{createOperation.FirstName} {createOperation.LastName}");

                bool createSuccess = await _canvasGateway.CreateUser(
                    createOperation.UserId, 
                    createOperation.FirstName, 
                    createOperation.LastName, 
                    $"{createOperation.PortalUsername}@education.nsw.gov.au", 
                    createOperation.EmailAddress, 
                    cancellationToken);

                if (createSuccess)
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

            case nameof(ModifyEnrolmentCanvasOperation):
                ModifyEnrolmentCanvasOperation modifyOperation = operation as ModifyEnrolmentCanvasOperation;

                _logger
                    .ForContext(nameof(ModifyEnrolmentCanvasOperation), modifyOperation, true)
                    .Information("Modifying Canvas enrollment for {name}", $"{modifyOperation.UserId}");

                bool modifySuccess = modifyOperation.Action switch
                {
                    _ when modifyOperation.Action.Equals(CanvasAction.Add) && modifyOperation.UserType.Equals(CanvasUserType.Teacher) =>
                        await _canvasGateway.EnrolToCourse(modifyOperation.UserId, CanvasCourseCode.FromValue(modifyOperation.CourseId), CanvasPermissionLevel.Teacher, cancellationToken),
                    _ when modifyOperation.Action.Equals(CanvasAction.Add) && modifyOperation.UserType.Equals(CanvasUserType.Student) && request.UseSections =>
                        await _canvasGateway.EnrolToSection(modifyOperation.UserId, CanvasSectionCode.FromValue(modifyOperation.CourseId), CanvasPermissionLevel.Student, cancellationToken),
                    _ when modifyOperation.Action.Equals(CanvasAction.Add) && modifyOperation.UserType.Equals(CanvasUserType.Student) && !request.UseSections =>
                        await _canvasGateway.EnrolToCourse(modifyOperation.UserId, CanvasCourseCode.FromValue(modifyOperation.CourseId), CanvasPermissionLevel.Student, cancellationToken),
                    _ when modifyOperation.Action.Equals(CanvasAction.Remove) =>
                        await _canvasGateway.UnenrolUser(modifyOperation.UserId, CanvasCourseCode.FromValue(modifyOperation.CourseId), cancellationToken),
                    _ => false
                };
                
                if (modifySuccess)
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
                    .Warning("Failed to modify Canvas enrollment for {name}", $"{modifyOperation.UserId}");

                return Result.Failure(DomainErrors.Operations.Canvas.ProcessFailed);

            case nameof(DeleteUserCanvasOperation):
                DeleteUserCanvasOperation deleteOperation = operation as DeleteUserCanvasOperation;

                _logger
                    .ForContext(nameof(DeleteUserCanvasOperation), deleteOperation, true)
                    .Information("Deleting Canvas user for {name}", $"{deleteOperation.UserId}");

                bool deleteSuccess = await _canvasGateway.DeactivateUser(deleteOperation.UserId, cancellationToken);

                if (deleteSuccess)
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
                    .Warning("Failed to delete Canvas user for {name}", $"{deleteOperation.UserId}");

                return Result.Failure(DomainErrors.Operations.Canvas.ProcessFailed);
        }

        return Result.Failure(DomainErrors.Operations.Canvas.Invalid);
    }
}