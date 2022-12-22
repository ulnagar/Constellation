namespace Constellation.Application.GroupTutorials.GetTutorialWithDetailsById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTutorialWithDetailsByIdQueryHandler
    : IQueryHandler<GetTutorialWithDetailsByIdQuery, GroupTutorialDetailResponse>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IStudentRepository _studentRepository;

    public GetTutorialWithDetailsByIdQueryHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IStaffRepository staffRepository,
        IStudentRepository studentRepository)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _staffRepository = staffRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<GroupTutorialDetailResponse>> Handle(GetTutorialWithDetailsByIdQuery request, CancellationToken cancellationToken)
    {
        var tutorial = await _groupTutorialRepository.GetWholeAggregate(request.Id, cancellationToken);

        if (tutorial is null)
        {
            return Result.Failure<GroupTutorialDetailResponse>(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.Id));
        }

        var teacherLinks = tutorial.Teachers.Where(teacher => !teacher.IsDeleted).ToList();
        var studentLinks = tutorial.CurrentEnrolments.ToList();

        var teacherEntities = await _staffRepository.GetListFromIds(teacherLinks.Select(teacher => teacher.StaffId).ToList(), cancellationToken);
        var studentEntities = await _studentRepository.GetListFromIds(studentLinks.Select(student => student.StudentId).ToList(), cancellationToken);

        var teachers = teacherLinks
            .Select(teacher => 
                new TutorialTeacherResponse(
                    teacher.Id, 
                    teacherEntities.First(entity => entity.StaffId == teacher.StaffId).DisplayName,
                    teacher.EffectiveTo))
            .ToList();

        var students = studentLinks
            .Select(student =>
                new TutorialEnrolmentResponse(
                    student.Id,
                    studentEntities.First(entity => entity.StudentId == student.StudentId).DisplayName,
                    studentEntities.First(entity => entity.StudentId == student.StudentId).CurrentGrade.AsName(),
                    student.EffectiveTo))
            .ToList();

        var response = new GroupTutorialDetailResponse(
            tutorial.Id,
            tutorial.Name,
            tutorial.StartDate,
            tutorial.EndDate,
            teachers,
            students,
            tutorial.Rolls
                .Select(roll => 
                    new TutorialRollResponse(
                        roll.Id, 
                        roll.SessionDate, 
                        !string.IsNullOrWhiteSpace(roll.StaffId), 
                        roll.Students.Count, 
                        roll.Students.Count(student => student.Present)))
                .ToList());

        return response;
    }
}
