namespace Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTutorialRollWithDetailsByIdQueryHandler
    : IQueryHandler<GetTutorialRollWithDetailsByIdQuery, TutorialRollDetailResponse>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly ITutorialRollRepository _tutorialRollRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetTutorialRollWithDetailsByIdQueryHandler(
        IGroupTutorialRepository groupTutorialRepository,
        ITutorialRollRepository tutorialRollRepository,
        IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _tutorialRollRepository = tutorialRollRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TutorialRollDetailResponse>> Handle(GetTutorialRollWithDetailsByIdQuery request, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository.GetWithRollsById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            return Result.Failure<TutorialRollDetailResponse>(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));
        }

        var roll = tutorial.Rolls.FirstOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
        {
            return Result.Failure<TutorialRollDetailResponse>(DomainErrors.GroupTutorials.TutorialRoll.NotFound(request.RollId));
        }

        var response = new TutorialRollDetailResponse(
            roll.Id,
            roll.SessionDate,
            roll.StaffId,
            roll.Status,
            roll.Students
                .Select(student => 
                    new TutorialRollStudentReponse(
                        student.StudentId, 
                        student.Enrolled, 
                        student.Present))
                .ToList());

        return response;
    }
}
