namespace Constellation.Application.Rollover.ImportStudents;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ImportStudentsCommandHandler
    : ICommandHandler<ImportStudentsCommand, List<ImportResult>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public ImportStudentsCommandHandler(
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger.ForContext<ImportStudentsCommand>();
    }

    public async Task<Result<List<ImportResult>>> Handle(ImportStudentsCommand request, CancellationToken cancellationToken)
    {
        List<ImportResult> results = new();

        foreach (StudentImportRecord entry in request.Records)
        {
            Student existingStudent = await _studentRepository.GetById(entry.StudentId, cancellationToken);

            if (existingStudent is not null)
            {
                switch (existingStudent.IsDeleted)
                {
                    case false:
                        _logger
                            .ForContext(nameof(StudentImportRecord), entry, true)
                            .ForContext(nameof(Error), StudentErrors.AlreadyExists(entry.StudentId), true)
                            .Warning("Failed to import new student");

                        results.Add(new(entry, Result.Failure(StudentErrors.AlreadyExists(entry.StudentId))));
                        break;
                    case true:
                        existingStudent.Reinstate(_dateTime);

                        results.Add(new(entry, Result.Success()));
                        break;
                }

                continue;
            }

            Student newStudent = Student.Create(
                entry.StudentId,
                entry.FirstName,
                entry.LastName,
                entry.PortalUsername,
                entry.Grade,
                entry.SchoolCode,
                entry.Gender);

            _studentRepository.Insert(newStudent);

            results.Add(new(entry, Result.Success()));
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return results;
    }
}
