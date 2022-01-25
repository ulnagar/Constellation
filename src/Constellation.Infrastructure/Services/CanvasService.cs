using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    public class CanvasService : ICanvasService, IScopedService
    {
        private readonly ICanvasGateway _canvasGateway;

        public CanvasService(ICanvasGateway canvasGateway)
        {
            _canvasGateway = canvasGateway;
        }

        public async Task<ServiceOperationResult<CanvasOperation>> ProcessOperation(CanvasOperation operation)
        {
            var result = new ServiceOperationResult<CanvasOperation>
            {
                Success = false
            };

            result.Errors.Add($" Processing operation {operation.Id}");

            switch (operation.GetType().Name)
            {
                case "CreateUserCanvasOperation":
                    var createOperation = operation as CreateUserCanvasOperation;

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
                        result.Errors.Add($" An error occured while processing operation with Id {operation.Id}");

                        return result;
                    }
                case "ModifyEnrolmentCanvasOperation":
                    var modifyOperation = operation as ModifyEnrolmentCanvasOperation;

                    bool modifySuccess = false;

                    if (modifyOperation.Action == CanvasOperation.EnrolmentAction.Add)
                    {
                        result.Errors.Add($"  Attempting to enrol user {modifyOperation.UserId} in course {modifyOperation.CourseId} as {modifyOperation.UserType}");

                        modifySuccess = await _canvasGateway.EnrolUser(modifyOperation.UserId, modifyOperation.CourseId, modifyOperation.UserType);
                    }

                    if (modifyOperation.Action == CanvasOperation.EnrolmentAction.Remove)
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
                        result.Errors.Add($" An error occured while processing operation with Id {operation.Id}");

                        return result;
                    }
                case "DeleteUserCanvasOperation":
                    var deleteOperation = operation as DeleteUserCanvasOperation;

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
                        result.Errors.Add($" An error occured while processing operation with Id {operation.Id}");

                        return result;
                    }
                default:
                    result.Errors.Add($" Could not determine which operation action was applicable");
                    return result;
            }
        }
    }
}
