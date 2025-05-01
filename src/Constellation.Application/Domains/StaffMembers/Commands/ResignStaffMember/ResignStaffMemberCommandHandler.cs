namespace Constellation.Application.Domains.StaffMembers.Commands.ResignStaffMember;

using Abstractions.Messaging;
using Application.Models.Auth;
using Application.Models.Identity;
using Core.Errors;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Repositories;
using Core.Shared;
using DTOs;
using Interfaces.Repositories;
using Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ResignStaffMemberCommandHandler
: ICommandHandler<ResignStaffMemberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IOperationService _operationsService;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ResignStaffMemberCommandHandler(
        IStaffRepository staffRepository,
        IOperationService operationsService,
        IFacultyRepository facultyRepository,
        IOfferingRepository offeringRepository,
        ITrainingModuleRepository moduleRepository,
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _operationsService = operationsService;
        _facultyRepository = facultyRepository;
        _offeringRepository = offeringRepository;
        _moduleRepository = moduleRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ResignStaffMemberCommand>();
    }

    //TODO: R1.15: Refactor these actions to Domain Events in Staff Aggregate

    public async Task<Result> Handle(ResignStaffMemberCommand request, CancellationToken cancellationToken)
    {
        Staff staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(ResignStaffMemberCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(request.StaffId), true)
                .Warning("Failed to mark Staff Member resigned");

            return Result.Failure(DomainErrors.Partners.Staff.NotFound(request.StaffId));
        }

        if (staffMember.IsDeleted)
            return Result.Success();

        staffMember.IsDeleted = true;
        staffMember.DateDeleted = DateTime.Now;

        List<Offering> offerings = await _offeringRepository.GetActiveForTeacher(request.StaffId, cancellationToken);

        foreach (Offering offering in offerings)
        {
            List<TeacherAssignment> assignments = offering
                .Teachers
                .Where(assignment =>
                    assignment.StaffId == request.StaffId &&
                    !assignment.IsDeleted)
                .ToList();

            foreach (TeacherAssignment assignment in assignments)
                offering.RemoveTeacher(request.StaffId, assignment.Type);
        }

        foreach (FacultyMembership membership in staffMember.Faculties)
        {
            if (membership.IsDeleted)
                continue;

            Faculty faculty = await _facultyRepository.GetById(membership.FacultyId, cancellationToken);
            faculty.RemoveMember(staffMember.StaffId);
        }

        await _operationsService.RemoveTeacherEmployedMSTeamAccess(staffMember.StaffId);

        await _operationsService.DisableCanvasUser(staffMember.StaffId);

        List<TrainingModule> modules = await _moduleRepository.GetModulesByAssignee(request.StaffId, cancellationToken);

        foreach (TrainingModule module in modules)
        {
            Result result = module.RemoveAssignee(request.StaffId);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(ResignStaffMemberCommand), request, true)
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to mark Staff Member resigned");

                return Result.Failure(result.Error);
            }
        }

        // Remove user access
        UserTemplateDto userDetails = new()
        {
            FirstName = staffMember.FirstName,
            LastName = staffMember.LastName,
            Email = staffMember.EmailAddress,
            Username = staffMember.EmailAddress,
            IsStaffMember = false
        };

        if (_userManager.Users.Any(u => u.UserName == userDetails.Username))
        {
            AppUser user = await _userManager.FindByEmailAsync(userDetails.Email);

            user!.IsStaffMember = false;
            user.StaffId = string.Empty;

            await _userManager.RemoveFromRoleAsync(user, AuthRoles.StaffMember);

            await _userManager.UpdateAsync(user);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
