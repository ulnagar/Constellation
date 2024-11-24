namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Abstractions;
using BaseModels;
using Constellation.Core.Models.Attachments.Services;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Models.ThirdPartyConsent.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Parents.Areas.Parents.Pages.Contacts;
using Serilog;
using System.Net.Mime;
using System.Threading;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IConsentRepository _consentRepository;
    private readonly IEmailAttachmentService _attachmentService;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        IConsentRepository consentRepository,
        IEmailAttachmentService attachmentService,
        ILogger logger)
    {
        _mediator = mediator;
        _consentRepository = consentRepository;
        _attachmentService = attachmentService;
        _logger = logger;
    }
    public async Task OnGet() { }


    public async Task<IActionResult> OnGetDownload()
    {
        ConsentTransactionId transactionId = ConsentTransactionId.FromValue(new Guid("570419F2-E5D1-43C2-8BDA-ABBE0A579231"));

        Transaction transaction = await _consentRepository.GetTransactionById(transactionId);

        var document = await _attachmentService.GenerateConsentTransactionReceipt(transaction);

        return File(document.ContentStream, MediaTypeNames.Application.Pdf, document.Name);
    }
}