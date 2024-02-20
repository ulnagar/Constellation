namespace Constellation.Application.Features.API.Operations.Commands;

using Core.Models.Operations;
using Core.Models.Operations.Enums;
using DTOs;
using Interfaces.Gateways;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ProcessCanvasOperationCommandHandler : IRequestHandler<ProcessCanvasOperationCommand, ServiceOperationResult<CanvasOperation>>
{
    private readonly ICanvasGateway _canvasGateway;

    public ProcessCanvasOperationCommandHandler() { }

    public ProcessCanvasOperationCommandHandler(ICanvasGateway canvasGateway)
    {
        _canvasGateway = canvasGateway;
    }

    public async Task<ServiceOperationResult<CanvasOperation>> Handle(ProcessCanvasOperationCommand request, CancellationToken cancellationToken)
    {
        var result = new ServiceOperationResult<CanvasOperation>
        {
            Success = false
        };

        if (_canvasGateway is null)
        {
            result.Errors.Add($" Canvas Gateway not available in this application!");
            return result;
        }

        result.Errors.Add($" Processing operation {request.Operation.Id}");

        switch (request.Operation.GetType().Name)
        {
            case "CreateUserCanvasOperation":
                var createOperation = request.Operation as CreateUserCanvasOperation;

                result.Errors.Add($"  Attempting to create Canvas user for {createOperation.FirstName} {createOperation.LastName}");
                var createSuccess = await _canvasGateway.CreateUser(createOperation.UserId, createOperation.FirstName, createOperation.LastName, $"{createOperation.PortalUsername}@education.nsw.gov.au", createOperation.EmailAddress);

                if (createSuccess)
                {
                    result.Errors.Add($" Successfully processed operation.");

                    createOperation.IsCompleted = true;

                    result.Success = true;
                    return result;
                }
                else
                {
                    result.Errors.Add($" An error occured while processing operation with Id {request.Operation.Id}");

                    return result;
                }
            case "ModifyEnrolmentCanvasOperation":
                var modifyOperation = request.Operation as ModifyEnrolmentCanvasOperation;

                bool modifySuccess = false;

                if (modifyOperation.Action.Equals(CanvasAction.Add))
                {
                    result.Errors.Add($"  Attempting to enrol user {modifyOperation.UserId} in course {modifyOperation.CourseId} as {modifyOperation.UserType}");

                    modifySuccess = await _canvasGateway.EnrolUser(modifyOperation.UserId, modifyOperation.CourseId, modifyOperation.UserType.Value);
                }

                if (modifyOperation.Action.Equals(CanvasAction.Remove))
                {
                    result.Errors.Add($"  Attempting to remove user {modifyOperation.UserId} from course {modifyOperation.CourseId}");

                    modifySuccess = await _canvasGateway.UnenrolUser(modifyOperation.UserId, modifyOperation.CourseId);
                }

                if (modifySuccess)
                {
                    result.Errors.Add($" Successfully processed operation.");

                    modifyOperation.IsCompleted = true;

                    result.Success = true;
                    return result;
                }
                else
                {
                    result.Errors.Add($" An error occured while processing operation with Id {request.Operation.Id}");

                    return result;
                }
            case "DeleteUserCanvasOperation":
                var deleteOperation = request.Operation as DeleteUserCanvasOperation;

                result.Errors.Add($"  Attempting to deactivate user {deleteOperation.UserId}");

                var deleteSuccess = await _canvasGateway.DeactivateUser(deleteOperation.UserId);

                if (deleteSuccess)
                {
                    result.Errors.Add($" Successfully processed operation.");

                    deleteOperation.IsCompleted = true;

                    result.Success = true;
                    return result;
                }
                else
                {
                    result.Errors.Add($" An error occured while processing operation with Id {request.Operation.Id}");

                    return result;
                }
            default:
                result.Errors.Add($" Could not determine which operation action was applicable");
                return result;
        }
    }
}