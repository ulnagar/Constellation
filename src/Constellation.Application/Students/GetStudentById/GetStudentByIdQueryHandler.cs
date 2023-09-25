namespace Constellation.Application.Students.GetStudentById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Students.Models;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
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
        var student = await _studentRepository.GetWithSchoolById(request.StudentId, cancellationToken);

        if (student is null)
        {
            return Result.Failure<StudentResponse>(DomainErrors.Partners.Student.NotFound(request.StudentId));
        }

        return new StudentResponse(
            student.StudentId,
            student.DisplayName,
            student.CurrentGrade,
            student.EmailAddress,
            student.School.Name,
            student.SchoolCode,
            student.IsDeleted);
    }
}
