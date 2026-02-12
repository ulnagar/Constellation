namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.AppSettings.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.StaffMembers.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IAppSettingsService appSettingsService,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _appSettingsService = appSettingsService;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task OnGet()
    {
        CoversConfiguration? configuration = await _appSettingsService.Covers();

        if (configuration is null)
            return;

        CoversConfiguration newConfig = configuration with { ContactTitle = "Daily Organiser" };

        await _appSettingsService.Covers(newConfig);
        await _unitOfWork.CompleteAsync();
    }

    public async Task OnGetCreate()
    {
        StaffMember? evan = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1226239"));
        StaffMember? karen = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1112830"));

        Dictionary<StaffMember, List<Grade>> members = new();
        List<Grade> grades = [Grade.Y05, Grade.Y06, Grade.Y07, Grade.Y08, Grade.Y09, Grade.Y10, Grade.Y11, Grade.Y12];

        members.Add(evan, grades);
        members.Add(karen, grades);

        CoversConfiguration configuration = new(
            evan.Name.DisplayName,
            "Casual Coordinator",
            "0412 225 129",
            members);

        var newConfig = configuration with { ContactTitle = "Daily Organiser" };

        await _appSettingsService.Covers(configuration);
        await _unitOfWork.CompleteAsync();
    }
}