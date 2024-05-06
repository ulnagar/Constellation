﻿namespace Constellation.Application.Features.API.Operations.Commands;

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
        ServiceOperationResult<CanvasOperation> result = new()
        {
            Success = false
        };

        if (_canvasGateway is null)
        {
            result.Errors.Add(" Canvas Gateway not available in this application!");
            return result;
        }

        result.Errors.Add($" Processing operation {request.Operation.Id}");

        switch (request.Operation.GetType().Name)
        {
            case "CreateUserCanvasOperation":
                CreateUserCanvasOperation createOperation = request.Operation as CreateUserCanvasOperation;

                result.Errors.Add($"  Attempting to create Canvas user for {createOperation!.FirstName} {createOperation.LastName}");
                bool createSuccess = await _canvasGateway.CreateUser(createOperation.UserId, createOperation.FirstName, createOperation.LastName, $"{createOperation.PortalUsername}@education.nsw.gov.au", createOperation.EmailAddress, cancellationToken);

                if (createSuccess)
                {
                    result.Errors.Add(" Successfully processed operation.");

                    createOperation.IsCompleted = true;

                    result.Success = true;
                    return result;
                }
                else
                {
                    result.Errors.Add($" An error occured while processing operation with Id {request.Operation.Id}");

                    return result;
                }
            case nameof(ModifyEnrolmentCanvasOperation):
                ModifyEnrolmentCanvasOperation modifyOperation = request.Operation as ModifyEnrolmentCanvasOperation;

                bool modifySuccess = false;

                if (modifyOperation!.Action.Equals(CanvasAction.Add))
                {
                    result.Errors.Add($"  Attempting to enrol user {modifyOperation.UserId} in course {modifyOperation.CourseId} as {modifyOperation.UserType}");

                    if (modifyOperation.UserType.Equals(CanvasUserType.Teacher))
                        modifySuccess = await _canvasGateway.EnrolToCourse(modifyOperation.UserId, modifyOperation.CourseId, modifyOperation.UserType.Value, cancellationToken);
                    else 
                        modifySuccess = await _canvasGateway.EnrolToSection(modifyOperation.UserId, modifyOperation.CourseId, modifyOperation.UserType.Value, cancellationToken);
                }

                if (modifyOperation.Action.Equals(CanvasAction.Remove))
                {
                    result.Errors.Add($"  Attempting to remove user {modifyOperation.UserId} from course {modifyOperation.CourseId}");

                    modifySuccess = await _canvasGateway.UnenrolUser(modifyOperation.UserId, modifyOperation.CourseId, cancellationToken);
                }

                if (modifySuccess)
                {
                    result.Errors.Add(" Successfully processed operation.");

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
                DeleteUserCanvasOperation deleteOperation = request.Operation as DeleteUserCanvasOperation;

                result.Errors.Add($"  Attempting to deactivate user {deleteOperation!.UserId}");

                bool deleteSuccess = await _canvasGateway.DeactivateUser(deleteOperation.UserId, cancellationToken);

                if (deleteSuccess)
                {
                    result.Errors.Add(" Successfully processed operation.");

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
                result.Errors.Add(" Could not determine which operation action was applicable");
                return result;
        }
    }
}