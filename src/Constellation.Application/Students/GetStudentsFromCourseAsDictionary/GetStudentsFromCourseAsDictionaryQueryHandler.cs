namespace Constellation.Application.Students.GetStudentsFromCourseAsDictionary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentsFromCourseAsDictionaryQueryHandler
    : IQueryHandler<GetStudentsFromCourseAsDictionaryQuery, Dictionary<string, string>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;

    public GetStudentsFromCourseAsDictionaryQueryHandler(
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        IEnrolmentRepository enrolmentRepository)
    {
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _enrolmentRepository = enrolmentRepository;
    }

    public async Task<Result<Dictionary<string, string>>> Handle(GetStudentsFromCourseAsDictionaryQuery request, CancellationToken cancellationToken)
    {
        Dictionary<string, string> results = new();

        var offerings = await _offeringRepository.GetByCourseId(request.CourseId, cancellationToken);

        if (offerings is null)
            return Result.Failure<Dictionary<string, string>>(CourseErrors.NoOfferings(request.CourseId));

        List<Enrolment> enrolments = new();

        foreach (var offering in offerings)
        {
            var valid = await _enrolmentRepository.GetCurrentByOfferingId(offering.Id, cancellationToken);

            if (valid is null)
                continue;

            enrolments.AddRange(valid);
        }

        enrolments = enrolments.Distinct().ToList();

        var students = await _studentRepository.GetListFromIds(enrolments.Select(entry => entry.StudentId).ToList(), cancellationToken);

        if (students is null)
            return results;

        foreach (var student in students.OrderBy(entry => entry.LastName))
        {
            results.Add(student.StudentId, $"{student.DisplayName} ({student.CurrentGrade.AsName()})");
        }

        return results;
    }
}
