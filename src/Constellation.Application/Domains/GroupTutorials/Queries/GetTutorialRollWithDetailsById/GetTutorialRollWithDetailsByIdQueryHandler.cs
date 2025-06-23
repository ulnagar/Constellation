namespace Constellation.Application.Domains.GroupTutorials.Queries.GetTutorialRollWithDetailsById;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Extensions;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using System.Collections.Generic;
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
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
            return Result.Failure<TutorialRollDetailResponse>(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));

        TutorialRoll roll = tutorial.Rolls.FirstOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
            return Result.Failure<TutorialRollDetailResponse>(DomainErrors.GroupTutorials.TutorialRoll.NotFound(request.RollId));

        string staffName = string.Empty;

        if (roll.Status == Core.Enums.TutorialRollStatus.Submitted)
        {
            StaffMember staffMember = await _staffRepository.GetById(roll.StaffId.Value, cancellationToken);

            staffName = staffMember?.Name.DisplayName;
        }

        List<Student> studentEntities = await _studentRepository.GetListFromIds(roll.Students.Select(student => student.StudentId).ToList(), cancellationToken);

        List<TutorialRollStudentResponse> students = roll.Students
            .Select(student =>
                new TutorialRollStudentResponse(
                    student.StudentId,
                    studentEntities.First(entity => entity.Id == student.StudentId).Name.DisplayName,
                    studentEntities.First(entity => entity.Id == student.StudentId).CurrentEnrolment?.Grade.AsName(),
                    student.Enrolled,
                    student.Present))
            .ToList();

        TutorialRollDetailResponse response = new(
            roll.Id,
            tutorial.Id,
            tutorial.Name,
            roll.SessionDate,
            roll.StaffId ?? StaffId.Empty, 
            staffName,
            roll.Status,
            students);

        return response;
    }
}
