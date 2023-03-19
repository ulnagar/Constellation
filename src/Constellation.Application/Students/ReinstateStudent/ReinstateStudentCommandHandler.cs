namespace Constellation.Application.Students.ReinstateStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReinstateStudentCommandHandler
    : ICommandHandler<ReinstateStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IOperationService _operationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ReinstateStudentCommandHandler(
        IStudentRepository studentRepository,
        IOperationService operationService,
        IUnitOfWork unitOfWork,
        Serilog.ILogger logger)
    {
        _studentRepository = studentRepository;
        _operationService = operationService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ReinstateStudentCommand>();
    }

    public async Task<Result> Handle(ReinstateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find student with Id {id}", request.StudentId);

            return Result.Failure(DomainErrors.Partners.Student.NotFound(request.StudentId));
        }

        if (!student.IsDeleted)
        {
            return Result.Success();
        }

        // Calculate new grade
        var yearLeft = student.DateDeleted.Value.Year;
        var previousGrade = (int)student.CurrentGrade;
        var thisYear = DateTime.Now.Year;
        var difference = thisYear - yearLeft;
        var thisGrade = previousGrade + difference;
        if (thisGrade > 12 || thisGrade == previousGrade)
        {
            // Do NOTHING!
        }
        else if (Enum.IsDefined(typeof(Grade), thisGrade))
        {
            var newGrade = (Grade)thisGrade;
            student.CurrentGrade = newGrade;
        }

        student.IsDeleted = false;
        student.DateDeleted = null;

        await _operationService.CreateStudentEnrolmentMSTeamAccess(student.StudentId);
        await _operationService.CreateCanvasUserFromStudent(student);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}