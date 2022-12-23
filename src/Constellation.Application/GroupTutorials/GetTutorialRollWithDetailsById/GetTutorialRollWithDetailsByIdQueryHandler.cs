namespace Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTutorialRollWithDetailsByIdQueryHandler
    : IQueryHandler<GetTutorialRollWithDetailsByIdQuery, TutorialRollDetailResponse>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;

    public GetTutorialRollWithDetailsByIdQueryHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
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

        string staffName = string.Empty;

        if (roll.Status == Core.Enums.TutorialRollStatus.Submitted)
        {
            var staffMember = await _staffRepository.GetForExistCheck(roll.StaffId);

            staffName = staffMember.DisplayName;
        }

        var studentEntities = await _studentRepository.GetListFromIds(roll.Students.Select(student => student.StudentId).ToList(), cancellationToken);

        var students = roll.Students
            .Select(student =>
                new TutorialRollStudentResponse(
                    student.StudentId,
                    studentEntities.First(entity => entity.StudentId == student.StudentId).DisplayName,
                    studentEntities.First(entity => entity.StudentId == student.StudentId).CurrentGrade.AsName(),
                    student.Enrolled,
                    student.Present))
            .ToList();

        var response = new TutorialRollDetailResponse(
            roll.Id,
            tutorial.Id,
            tutorial.Name,
            roll.SessionDate,
            roll.StaffId,
            staffName,
            roll.Status,
            students);

        return response;
    }
}
