namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Interfaces.Repositories;
using BaseModels;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IConsentRepository _consentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(
        IMediator mediator,
        IConsentRepository consentRepository,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _consentRepository = consentRepository;
        _unitOfWork = unitOfWork;
    }

    public List<Application> Applications { get; set; } = new();
    public List<Transaction> Transactions { get; set; } = new();

    public async Task OnGet()
    {
        Applications = await _consentRepository.GetAllActiveApplications();
        Transactions = await _consentRepository.GetAllTransactions();
    }

    public async Task<IActionResult> OnGetAddDummyData()
    {
        //Application app1 = Application.Create(
        //    "Canvas",
        //    "Platform for submission of student assessments",
        //    new string[]
        //    {
        //        "First Name",
        //        "Surname",
        //        "Email address",
        //        "Submitted works"
        //    },
        //    "Australia",
        //    new string[]
        //    {
        //        "TurnItIn",
        //        "Respondus LockDown Browser"
        //    },
        //    true);

        //_consentRepository.Insert(app1);

        //Application app2 = Application.Create(
        //    "Box of Books",
        //    "Platform for digital textbooks, including ability to view offline",
        //    new string[]
        //    {
        //        "First name",
        //        "Surname",
        //        "Email address",
        //        "Year level",
        //        "School name"
        //    },
        //    "Australia",
        //    new string[]
        //    {
        //        "Amazon Web Services",
        //        "Adobe Digital Rights Management",
        //        "Google",
        //        "Microsoft"
        //    },
        //    true);

        //_consentRepository.Insert(app2);

        //await _unitOfWork.CompleteAsync();

        List<Application> apps = await _consentRepository.GetAllActiveApplications();

        var app1 = apps.First(entry => entry.Name == "Box of Books");
        var app2 = apps.First(entry => entry.Name == "Canvas");

        Result<Transaction> transactionAttempt = Transaction.Create(
            "451543156",
            "jodie_minett@hotmail.com",
            DateTime.Now,
            ConsentMethod.Portal,
            string.Empty,
            new Dictionary<ApplicationId, bool>() { { app1.Id, true }, { app2.Id, false } });

        if (transactionAttempt.IsSuccess)
        {
            _consentRepository.Insert(transactionAttempt.Value);
            await _unitOfWork.CompleteAsync();
        }

        return RedirectToPage();
    }
}