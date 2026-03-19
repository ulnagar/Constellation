namespace Constellation.Application.Domains.Attendance.Absences.Commands.SetAbsenceConfigurationForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.Identifiers;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SetAbsenceConfigurationForStudentCommandHandler
    : ICommandHandler<SetAbsenceConfigurationForStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SetAbsenceConfigurationForStudentCommandHandler(
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<SetAbsenceConfigurationForStudentCommand>();
    }

    public async Task<Result> Handle(SetAbsenceConfigurationForStudentCommand request, CancellationToken cancellationToken)
    {

        // If a StudentId is present, process it first
        if (request.StudentId != StudentId.Empty)
        {
            Student? student = await _studentRepository.GetById(request.StudentId, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                    .Warning("Failed to add Absence Configuration for student");

                return Result.Failure(StudentErrors.NotFound(request.StudentId));
            }

            Result<AbsenceConfiguration> configRequest = AbsenceConfiguration.Create(
                request.StudentId,
                request.AbsenceType,
                request.StartDate,
                request.EndDate);

            if (configRequest.IsFailure)
            {
                _logger
                    .ForContext("Error", configRequest.Error)
                    .Warning("Failed to create Absence Configuration for student {student}", student.Name.DisplayName);

                return Result.Failure(configRequest.Error);
            }

            Result studentRequest = student.AddAbsenceConfiguration(configRequest.Value);

            if (studentRequest.IsFailure)
            {
                _logger
                    .ForContext("Error", studentRequest.Error)
                    .Warning("Failed to add Absence Configuration for student");

                return Result.Failure(studentRequest.Error);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }


        // If a School and Grade is selected, process it next
        if (request.SchoolCode != SchoolCode.Empty && request.GradeFilter is not null)
        {
            List<Student> students = await _studentRepository.GetCurrentStudentsFromSchool(request.SchoolCode, cancellationToken);

            students = students
                .Where(student => student.CurrentEnrolment?.Grade == (Grade)request.GradeFilter)
                .ToList();

            foreach (Student student in students)
            {
                Result<AbsenceConfiguration> configRequest = AbsenceConfiguration.Create(
                    student.Id,
                    request.AbsenceType,
                    request.StartDate,
                    request.EndDate);

                if (configRequest.IsFailure)
                {
                    _logger
                        .ForContext("Error", configRequest.Error)
                        .Warning("Failed to create Absence Configuration for student {student}", student.Name.DisplayName);

                    return Result.Failure(configRequest.Error);
                }

                Result studentRequest = student.AddAbsenceConfiguration(configRequest.Value);

                if (studentRequest.IsFailure)
                {
                    _logger
                        .ForContext("Error", studentRequest.Error)
                        .Warning("Failed to add Absence Configuration for student");

                    return Result.Failure(studentRequest.Error);
                }
            }

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }

        // If only a school is selected, process it next
        if (request.SchoolCode != SchoolCode.Empty)
        {
            List<Student> students = await _studentRepository.GetCurrentStudentsFromSchool(request.SchoolCode, cancellationToken);

            foreach (Student student in students)
            {
                Result<AbsenceConfiguration> configRequest = AbsenceConfiguration.Create(
                    student.Id,
                    request.AbsenceType,
                    request.StartDate,
                    request.EndDate);

                if (configRequest.IsFailure)
                {
                    _logger
                        .ForContext("Error", configRequest.Error)
                        .Warning("Failed to create Absence Configuration for student {student}", student.Name.DisplayName);

                    return Result.Failure(configRequest.Error);
                }

                Result studentRequest = student.AddAbsenceConfiguration(configRequest.Value);

                if (studentRequest.IsFailure)
                {
                    _logger
                        .ForContext("Error", studentRequest.Error)
                        .Warning("Failed to add Absence Configuration for student");

                    return Result.Failure(studentRequest.Error);
                }
            }

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }

        // If only a grade is selected, process that last
        if (request.GradeFilter is not null)
        {
            Grade grade = (Grade)request.GradeFilter.Value;

            List<Student> students = await _studentRepository.GetCurrentStudentFromGrade(grade, cancellationToken);

            foreach (Student student in students)
            {
                Result<AbsenceConfiguration> configRequest = AbsenceConfiguration.Create(
                    student.Id,
                    request.AbsenceType,
                    request.StartDate,
                    request.EndDate);

                if (configRequest.IsFailure)
                {
                    _logger
                        .ForContext("Error", configRequest.Error)
                        .Warning("Failed to create Absence Configuration for student {student}", student.Name.DisplayName);

                    return Result.Failure(configRequest.Error);
                }

                Result studentRequest = student.AddAbsenceConfiguration(configRequest.Value);

                if (studentRequest.IsFailure)
                {
                    _logger
                        .ForContext("Error", studentRequest.Error)
                        .Warning("Failed to add Absence Configuration for student");

                    return Result.Failure(studentRequest.Error);
                }
            }

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Result.Success();
        }

        return Result.Failure(StudentErrors.InvalidId);
    }
}
