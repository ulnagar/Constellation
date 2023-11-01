namespace Constellation.Application.GroupTutorials.GetTutorialWithDetailsById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Shared;
using Core.Extensions;
using System.Collections.Generic;
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
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(request.Id, cancellationToken);

        if (tutorial is null)
            return Result.Failure<GroupTutorialDetailResponse>(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.Id));

        List<TutorialTeacher> teacherLinks = tutorial.Teachers.Where(teacher => !teacher.IsDeleted).ToList();
        List<TutorialEnrolment> studentLinks = tutorial.CurrentEnrolments.ToList();

        List<Staff> teacherEntities = await _staffRepository.GetListFromIds(teacherLinks.Select(teacher => teacher.StaffId).ToList(), cancellationToken);
        List<Student> studentEntities = await _studentRepository.GetListFromIds(studentLinks.Select(student => student.StudentId).ToList(), cancellationToken);

        List<TutorialTeacherResponse> teachers = teacherLinks
            .Select(teacher => 
                new TutorialTeacherResponse(
                    teacher.Id, 
                    teacherEntities.First(entity => entity.StaffId == teacher.StaffId).DisplayName,
                    teacher.EffectiveTo))
            .ToList();

        List<TutorialEnrolmentResponse> students = studentLinks
            .Select(student =>
                new TutorialEnrolmentResponse(
                    student.Id,
                    studentEntities.First(entity => entity.StudentId == student.StudentId).DisplayName,
                    studentEntities.First(entity => entity.StudentId == student.StudentId).CurrentGrade.AsName(),
                    student.EffectiveTo))
            .ToList();

        GroupTutorialDetailResponse response = new(
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
