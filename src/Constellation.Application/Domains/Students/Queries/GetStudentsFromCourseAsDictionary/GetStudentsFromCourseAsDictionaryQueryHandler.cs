﻿namespace Constellation.Application.Domains.Students.Queries.GetStudentsFromCourseAsDictionary;

using Abstractions.Messaging;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects.Errors;
using Core.Extensions;
using Core.Models.Enrolments.Repositories;
using Core.Models.Offerings;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentsFromCourseAsDictionaryQueryHandler
    : IQueryHandler<GetStudentsFromCourseAsDictionaryQuery, Dictionary<StudentId, string>>
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

    public async Task<Result<Dictionary<StudentId, string>>> Handle(GetStudentsFromCourseAsDictionaryQuery request, CancellationToken cancellationToken)
    {
        Dictionary<StudentId, string> results = new();

        List<Offering> offerings = await _offeringRepository.GetByCourseId(request.CourseId, cancellationToken);

        if (offerings is null)
            return Result.Failure<Dictionary<StudentId, string>>(CourseErrors.NoOfferings(request.CourseId));

        List<Enrolment> enrolments = new();

        foreach (Offering offering in offerings)
        {
            List<Enrolment> valid = await _enrolmentRepository.GetCurrentByOfferingId(offering.Id, cancellationToken);

            if (valid is null)
                continue;

            enrolments.AddRange(valid);
        }

        enrolments = enrolments.Distinct().ToList();

        List<Student> students = await _studentRepository.GetListFromIds(enrolments.Select(entry => entry.StudentId).ToList(), cancellationToken);

        if (students is null)
            return results;

        foreach (Student student in students.OrderBy(entry => entry.Name.SortOrder))
        {
            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null) 
                continue;

            results.Add(student.Id, $"{student.Name.DisplayName} ({enrolment.Grade.AsName()})");
        }

        return results;
    }
}
