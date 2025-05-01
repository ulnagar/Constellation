namespace Constellation.Application.Domains.GroupTutorials.Queries.GetCurrentStudentsInGroupTutorial;

using Abstractions.Messaging;
using Constellation.Application.Domains.Students.Models;
using Constellation.Core.Models.Students;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models.GroupTutorials;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentStudentsInGroupTutorialQueryHandler
: IQueryHandler<GetCurrentStudentsInGroupTutorialQuery, List<StudentResponse>>
{
    private readonly IGroupTutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetCurrentStudentsInGroupTutorialQueryHandler(
        IGroupTutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetCurrentStudentsInGroupTutorialQuery>();
    }

    public async Task<Result<List<StudentResponse>>> Handle(GetCurrentStudentsInGroupTutorialQuery request, CancellationToken cancellationToken)
    {
        List<StudentResponse> response = new();

        GroupTutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(GetCurrentStudentsInGroupTutorialQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId), true)
                .Warning("Failed to retrieve student enrolments in group tutorial");

            return Result.Failure<List<StudentResponse>>(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));
        }

        List<StudentId> studentIds = tutorial.CurrentEnrolments
            .Select(entry => entry.StudentId)
            .ToList();

        List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

        foreach (Student student in students)
        {
            response.Add(new(
                student.Id,
                student.StudentReferenceNumber,
                student.Name,
                student.PreferredGender,
                student.CurrentEnrolment?.Grade,
                student.EmailAddress,
                student.CurrentEnrolment?.SchoolName,
                student.CurrentEnrolment?.SchoolCode,
                student.CurrentEnrolment is not null,
                student.IsDeleted));
        }

        return response;
    }
}
