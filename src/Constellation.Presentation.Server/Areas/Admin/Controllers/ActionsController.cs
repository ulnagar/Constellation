using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.Areas.Admin.Models;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActionsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdobeConnectService _adobeConnectService;
        private readonly IOperationService _operationService;
        private readonly ICasualRepository _casualRepository;

        public ActionsController(IUnitOfWork unitOfWork, IConfiguration configuration,
            IAdobeConnectService adobeConnectService, IOperationService operationService,
            ICasualRepository casualRepository)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _adobeConnectService = adobeConnectService;
            _operationService = operationService;
            _casualRepository = casualRepository;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await CreateViewModel<Actions_ViewModel>();

            var operations = await _unitOfWork.AdobeConnectOperations.AllRecentAsync();
            foreach (var operation in operations)
            {
                switch (operation)
                {
                    case StudentAdobeConnectOperation sOperation:
                        var sdto = new Actions_OperationDTO
                        {
                            Id = operation.Id,
                            UserType = "Student",
                            UserId = sOperation.StudentId,
                            UserName = sOperation.Student.DisplayName,
                            ActionType = operation.Action.ToString(),
                            RoomId = operation.ScoId,
                            RoomName = operation.Room.Name,
                            DateScheduled = operation.DateScheduled,
                            IsCompleted = operation.IsCompleted,
                            IsDeleted = operation.IsDeleted
                        };

                        viewModel.Operations.Add(sdto);

                        break;
                    case TeacherAdobeConnectOperation tOperation:
                        var tdto = new Actions_OperationDTO
                        {
                            Id = operation.Id,
                            UserType = "Teacher",
                            UserId = tOperation.StaffId,
                            UserName = tOperation.Teacher.DisplayName,
                            ActionType = operation.Action.ToString(),
                            RoomId = operation.ScoId,
                            RoomName = operation.Room.Name,
                            DateScheduled = operation.DateScheduled,
                            IsCompleted = operation.IsCompleted,
                            IsDeleted = operation.IsDeleted
                        };

                        viewModel.Operations.Add(tdto);

                        break;
                    case CasualAdobeConnectOperation cOperation:
                        var casual = await _casualRepository.GetById(CasualId.FromValue(cOperation.CasualId));
                        var cdto = new Actions_OperationDTO
                        {
                            Id = operation.Id,
                            UserType = "Casual",
                            UserId = cOperation.CasualId.ToString(),
                            UserName = casual.DisplayName,
                            ActionType = operation.Action.ToString(),
                            RoomId = operation.ScoId,
                            RoomName = operation.Room.Name,
                            DateScheduled = operation.DateScheduled,
                            IsCompleted = operation.IsCompleted,
                            IsDeleted = operation.IsDeleted
                        };

                        viewModel.Operations.Add(cdto);

                        break;

                    case TeacherAdobeConnectGroupOperation gOperation:
                        var gdto = new Actions_OperationDTO
                        {
                            Id = operation.Id,
                            UserType = "Group",
                            UserId = gOperation.TeacherId,
                            UserName = gOperation.Teacher.DisplayName,
                            ActionType = operation.Action.ToString(),
                            RoomId = operation.GroupSco,
                            RoomName = gOperation.GroupName,
                            DateScheduled = operation.DateScheduled,
                            IsCompleted = operation.IsCompleted,
                            IsDeleted = operation.IsDeleted
                        };

                        viewModel.Operations.Add(gdto);

                        break;

                    default:
                        break;
                }
            }

            viewModel.Operations = viewModel.Operations.OrderBy(o => o.DateScheduled).ToList();

            return View(viewModel);
        }

        public async Task<ActionResult> Process(int operationId)
        {
            var viewModel = await CreateViewModel<Actions_ProcessViewModel>();

            var operation = await _unitOfWork.AdobeConnectOperations.ForProcessingAsync(operationId);
            if (operation != null)
            {
                var result = await _adobeConnectService.ProcessOperation(operation);
                if (result.Success)
                    await _operationService.MarkAdobeConnectOperationCompleteAsync(operation.Id);

                viewModel.Success = result.Success;
                viewModel.Statuses = result.Errors;
                await _unitOfWork.CompleteAsync();
                return View(viewModel);
            }

            return RedirectToAction("Index");
        }
    }
}
