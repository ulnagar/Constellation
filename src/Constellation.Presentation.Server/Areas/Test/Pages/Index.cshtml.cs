namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.Messaging.Sms.Dtos;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.Auth;
using BaseModels;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Sms.Enums;
using Core.Models.Messaging.Sms.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly ISMSService _smsService;
    private readonly ISmsRepository _smsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISMSService smsService,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger logger, ISmsRepository smsRepository, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
        _smsRepository = smsRepository;
        _unitOfWork = unitOfWork;
        _smsService = smsService;
    }

    [BindProperty] public string From { get; set; }
    [BindProperty] public string To { get; set; }
    [BindProperty] public string Message { get; set; }

    public async Task OnGet()
    {

    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        OutgoingSms outgoingSms = new()
        {
            origin = From,
            destinations = To.Split(',').Select(number => number.Replace(" ", "")).ToList(),
            message = Message,
            notifyUrl = "json+https://acos.aurora.nsw.edu.au/api/sms"
        };

        Result<List<OutgoingSmsConfirmation>> results = await _smsService.SendMessage(outgoingSms, cancellationToken);

        foreach (OutgoingSmsConfirmation confirmation in results.Value)
        {
            SmsMessage message = new()
            {
                SmsGlobalId = confirmation.Id ?? string.Empty,
                SendingModule = "Testing",
                OutgoingId = confirmation.OutgoingId ?? string.Empty,
                From = confirmation.Origin ?? string.Empty,
                To = confirmation.Destination ?? string.Empty,
                Message = confirmation.Message ?? string.Empty,
                Direction = MessageDirection.Outbound,
                Status = SmsStatus.Sent,
                CreatedAt = confirmation.DateTime
            };

            _smsRepository.Insert(message);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return RedirectToPage();
    }
}