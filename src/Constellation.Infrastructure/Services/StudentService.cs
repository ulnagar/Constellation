namespace Constellation.Infrastructure.Services;

using Constellation.Application.DTOs;
using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Students;

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public StudentService(
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<ServiceOperationResult<Student>> UpdateStudent(string studentId, StudentDto studentResource)
    {
        // Set up return object
        var result = new ServiceOperationResult<Student>();

        // Validate entries
        var student = await _unitOfWork.Students.ForEditAsync(studentId);

        if (student == null)
        {
            result.Success = false;
            result.Errors.Add($"A student with that ID could not be found!");

            return result;
        }

        // Update properties
        if (!string.IsNullOrWhiteSpace(studentResource.FirstName))
            student.FirstName = studentResource.FirstName;

        if (!string.IsNullOrWhiteSpace(studentResource.LastName))
            student.LastName = studentResource.LastName;

        if (!string.IsNullOrWhiteSpace(studentResource.PortalUsername))
            student.PortalUsername = studentResource.PortalUsername;

        if (!string.IsNullOrWhiteSpace(studentResource.AdobeConnectPrincipalId))
            student.AdobeConnectPrincipalId = studentResource.AdobeConnectPrincipalId;

        if (!string.IsNullOrWhiteSpace(studentResource.SentralStudentId))
            student.SentralStudentId = studentResource.SentralStudentId;

        if (studentResource.CurrentGrade != 0)
            student.CurrentGrade = studentResource.CurrentGrade;

        if (studentResource.EnrolledGrade != 0)
            student.EnrolledGrade = studentResource.EnrolledGrade;

        if (!string.IsNullOrWhiteSpace(studentResource.Gender))
            student.Gender = studentResource.Gender;

        if (!string.IsNullOrWhiteSpace(studentResource.SchoolCode))
        {
            if (student.SchoolCode != studentResource.SchoolCode)
            {
                await _mediator.Publish(new StudentMovedSchoolsNotification { StudentId = student.StudentId, OldSchoolCode = student.SchoolCode, NewSchoolCode = studentResource.SchoolCode });
                student.SchoolCode = studentResource.SchoolCode;
            }
        }

        result.Success = true;
        result.Entity = student;

        return result;
    }
}