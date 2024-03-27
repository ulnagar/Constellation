namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Interfaces.Repositories;
using BaseModels;
using Core.Models.Attendance.Identifiers;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Repositories;
using Core.Models.WorkFlow.Services;
using Core.Shared;
using MediatR;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICaseRepository _caseRepository;
    private readonly ICaseService _caseService;
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(
        IMediator mediator,
        ICaseRepository caseRepository,
        ICaseService caseService,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _caseRepository = caseRepository;
        _caseService = caseService;
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

}