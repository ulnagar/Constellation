namespace Constellation.Application.Absences.SetAbsenceConfigurationForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Core.Models.Students.Errors;
using Serilog;
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
        if (!string.IsNullOrWhiteSpace(request.SchoolCode))
        {
            var students = await _studentRepository.GetCurrentStudentsFromSchool(request.SchoolCode, cancellationToken);

            if (request.GradeFilter is not null)
            {
                students = students
                    .Where(student => student.CurrentGrade == (Grade)request.GradeFilter)
                    .ToList();
            }

            foreach (var student in students)
            {
                Result<AbsenceConfiguration> configRequest = AbsenceConfiguration.Create(
                    student.StudentId,
                    request.AbsenceType,
                    request.StartDate,
                    request.EndDate);

                if (configRequest.IsFailure)
                {
                    _logger
                        .ForContext("Error", configRequest.Error)
                        .Warning("Failed to create Absence Configuration for student {student}", student.DisplayName);

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

        if (!string.IsNullOrWhiteSpace(request.StudentId))
        {
            var student = await _studentRepository.GetById(request.StudentId, cancellationToken);

            Result<AbsenceConfiguration> configRequest = AbsenceConfiguration.Create(
                student.StudentId,
                request.AbsenceType,
                request.StartDate,
                request.EndDate);

            if (configRequest.IsFailure)
            {
                _logger
                    .ForContext("Error", configRequest.Error)
                    .Warning("Failed to create Absence Configuration for student {student}", student.DisplayName);

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

        return Result.Failure(StudentErrors.InvalidId);
    }
}
