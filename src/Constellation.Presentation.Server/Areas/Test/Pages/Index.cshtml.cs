namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Interfaces.Repositories;
using BaseModels;
using Core.Abstractions.Services;
using Core.Models.Attendance.Identifiers;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Identifiers;
using Core.Models.WorkFlow.Repositories;
using Core.Models.WorkFlow.Services;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICaseRepository _caseRepository;
    private readonly ICaseService _caseService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(
        IMediator mediator,
        ICaseRepository caseRepository,
        ICaseService caseService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _caseRepository = caseRepository;
        _caseService = caseService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public List<Case> Cases { get; set; } = new();


    public async Task OnGet()
    {
        Cases = await _caseRepository.GetAll();
    }

    public async Task OnGetCreateCase()
    {
        // AttendanceValueId 2F8178D5-FB1B-4982-BF2A-12EC81DDB5E0
        // StudentId 444998822

        AttendanceValueId valueId = AttendanceValueId.FromValue(Guid.Parse("2F8178D5-FB1B-4982-BF2A-12EC81DDB5E0"));

        Result<Case> caseResult = await _caseService.CreateAttendanceCase("444998822", valueId);

        if (caseResult.IsSuccess)
            _caseRepository.Insert(caseResult.Value);

        await _unitOfWork.CompleteAsync();

        Cases.Add(caseResult.Value);
    }

    public async Task<IActionResult> OnGetUpdateAction(Guid caseId, Guid actionId)
    {
        Case item = await _caseRepository.GetById(CaseId.FromValue(caseId));

        if (item is null)
            return RedirectToPage();

        Action action = item.Actions.FirstOrDefault(action => action.Id == ActionId.FromValue(actionId));

        if (action is null) 
            return RedirectToPage();

        if (action is SendEmailAction emailAction)
        {
            List<EmailRecipient> recipients = new();

            Result<EmailRecipient> recipient1 = EmailRecipient.Create("John Smith", "john.smith@mybusiness.com.au");
            Result<EmailRecipient> recipient2 = EmailRecipient.Create("Emily Smith", "esmith11@gmail.com");

            recipients.Add(recipient1.Value);
            recipients.Add(recipient2.Value);

            Result<EmailRecipient> sender = EmailRecipient.Create("Ben Hillsley", "benjamin.hillsley@det.nsw.edu.au");

            Result update = emailAction.Update(
                recipients,
                sender.Value,
                "An update on your childs attendance",
                "He didn't attend",
                false,
                DateTime.Now,
                _currentUserService.UserName);

            if (update.IsFailure)
            {
                return RedirectToPage();
            }

            Result complete = item.UpdateActionStatus(emailAction.Id, ActionStatus.Completed, _currentUserService.UserName);

            if (complete.IsFailure)
            {
                return RedirectToPage();
            }
        }

        if (action is CreateSentralEntryAction sentralAction)
        {
            Result update = sentralAction.Update(true, _currentUserService.UserName);

            if (update.IsFailure)
            {
                return RedirectToPage();
            }

            Result complete = item.UpdateActionStatus(sentralAction.Id, ActionStatus.Completed, _currentUserService.UserName);

            if (complete.IsFailure)
            {
                return RedirectToPage();
            }
        }

        if (action is ConfirmSentralEntryAction confirmAction)
        {
            Result update = confirmAction.Update(true, _currentUserService.UserName);

            if (update.IsFailure)
            {
                return RedirectToPage();
            }

            Result complete = item.UpdateActionStatus(confirmAction.Id, ActionStatus.Completed, _currentUserService.UserName);

            if (complete.IsFailure)
            {
                return RedirectToPage();
            }
        }

        await _unitOfWork.CompleteAsync();

        return RedirectToPage();
    }

}