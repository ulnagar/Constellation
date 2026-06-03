namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Interfaces.Repositories;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Models.Common.Enums;
using Core.Models.StudentOnboarding;
using Core.Models.StudentOnboarding.Enums;
using Core.Models.StudentOnboarding.Repositories;
using Core.Models.StudentOnboarding.Services;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IOnboardingRepository _onboardingRepository;
    private readonly IOnboardingService _onboardingService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        IOnboardingRepository onboardingRepository,
        IOnboardingService onboardingService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _mediator = mediator;
        _onboardingRepository = onboardingRepository;
        _onboardingService = onboardingService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [ViewData] public string ActivePage => "";

    public async Task OnGet()
    {
        Result<Applicant> applicant = await _onboardingService.ApplicantFactory(
            null,
            Name.Create("John", "Jonny", "Smith").Value,
            EmailAddress.Create("jonny.smith123@education.nsw.gov.au").Value,
            Gender.Male,
            IndigenousStatus.NeitherAboriginalNorTorresStraitIslander);

        Result <Application> application = Application.Create(
            applicant.Value,
            Program.YoungAndDeadlyMob,
            "2027",
            Grade.Y08,
            null);

        _onboardingRepository.Insert(application.Value);

        await _unitOfWork.CompleteAsync();
    }
}