namespace Constellation.Infrastructure.Services;

using Application.Enums;
using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models;
using Core.Models.Operations;
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

    public async Task CreateTeacherEmployedMSTeamAccess(string staffId)
    {
        // Validate entries
        Staff staffMember = await _staffRepository.GetForExistCheck(staffId);

        if (staffMember == null)
            return;

        // Create Operation
        TeacherEmployedMSTeamOperation studentTeamOperation = new()
        {
            StaffId = staffId,
            TeamName = MicrosoftTeam.Students,
            Action = MSTeamOperationAction.Add,
            DateScheduled = _dateTime.Now,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _teamOperationRepository.Insert(studentTeamOperation);

        TeacherEmployedMSTeamOperation schoolTeamOperation = new()
        {
            StaffId = staffId,
            TeamName = MicrosoftTeam.Staff,
            Action = MSTeamOperationAction.Add,
            DateScheduled = _dateTime.Now,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _teamOperationRepository.Insert(schoolTeamOperation);
        await _unitOfWork.CompleteAsync();
    }

    public async Task RemoveTeacherEmployedMSTeamAccess(string staffId)
    {
        // Validate entries
        Staff staffMember = await _staffRepository.GetForExistCheck(staffId);

        if (staffMember == null)
            return;

        // Create Operation
        TeacherEmployedMSTeamOperation studentTeamOperation = new()
        {
            StaffId = staffId,
            TeamName = MicrosoftTeam.Students,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Member,
            DateScheduled = _dateTime.Now,
        };

        _teamOperationRepository.Insert(studentTeamOperation);

        TeacherEmployedMSTeamOperation schoolTeamOperation = new()
        {
            StaffId = staffId,
            TeamName = MicrosoftTeam.Staff,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Member,
            DateScheduled = _dateTime.Now,
        };

        _teamOperationRepository.Insert(schoolTeamOperation);
        await _unitOfWork.CompleteAsync();
    }
    
    public async Task CreateCanvasUserFromStaff(Staff staff)
    {
        if (staff is null)
            return;

        CreateUserCanvasOperation operation = new(
            staff.StaffId,
            staff.FirstName,
            staff.LastName,
            staff.PortalUsername,
            staff.EmailAddress);

        _canvasOperationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync();
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