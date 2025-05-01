namespace Constellation.Application.Domains.Students.Queries.GetStudentById;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Models;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentByIdQueryHandler
    : IQueryHandler<GetStudentByIdQuery, StudentResponse>
{
    private readonly IStudentRepository _studentRepository;

    public GetStudentByIdQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<StudentResponse>> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
            return Result.Failure<StudentResponse>(StudentErrors.NotFound(request.StudentId));

        SchoolEnrolment enrolment = student.CurrentEnrolment;

        if (enrolment is null)
        {
            return new StudentResponse(
                student.Id,
                student.StudentReferenceNumber ?? StudentReferenceNumber.Empty,
                student.Name,
                student.PreferredGender,
                null,
                student.EmailAddress,
                null,
                null,
                false,
                student.IsDeleted);
        }

        return new StudentResponse(
            student.Id,
            student.StudentReferenceNumber ?? StudentReferenceNumber.Empty,
            student.Name,
            student.PreferredGender,
            enrolment.Grade,
            student.EmailAddress,
            enrolment.SchoolName,
            enrolment.SchoolCode,
            true,
            student.IsDeleted);
    }
}
