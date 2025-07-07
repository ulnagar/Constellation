namespace Constellation.Infrastructure.Services;

using Application.Enums;
using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models;
using Core.Models.Operations;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using System.Threading.Tasks;

public class OperationService : IOperationService
{
    private readonly IMSTeamOperationsRepository _teamOperationRepository;
    private readonly ICanvasOperationsRepository _canvasOperationsRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public OperationService(
        IMSTeamOperationsRepository teamOperationRepository,
        ICanvasOperationsRepository canvasOperationsRepository,
        IStaffRepository staffRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork)
    {
        _teamOperationRepository = teamOperationRepository;
        _canvasOperationsRepository = canvasOperationsRepository;
        _staffRepository = staffRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task RemoveTeacherEmployedMSTeamAccess(StaffId staffId)
    {

    }
    
    public async Task DisableCanvasUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return;

        DeleteUserCanvasOperation operation = new(userId);

        _canvasOperationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync();
    }
}