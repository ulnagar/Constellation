namespace Constellation.Application.Students.GetLifecycleDetailsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Core.Errors;
using Core.Models;
using Core.Shared;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetLifecycleDetailsForStudentQueryHandler
    : IQueryHandler<GetLifecycleDetailsForStudentQuery, RecordLifecycleDetailsResponse>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetLifecycleDetailsForStudentQueryHandler(
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetLifecycleDetailsForStudentQuery>();
    }

    public async Task<Result<RecordLifecycleDetailsResponse>> Handle(GetLifecycleDetailsForStudentQuery request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(GetLifecycleDetailsForStudentQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Student.NotFound(request.StudentId), true)
                .Warning("Failed to retrieve record lifecycle details for Student");

            return Result.Failure<RecordLifecycleDetailsResponse>(DomainErrors.Partners.Student.NotFound(request.StudentId));
        }

        return new RecordLifecycleDetailsResponse(
            string.Empty,
            student.DateEntered ?? DateTime.MinValue,
            string.Empty,
            DateTime.MinValue,
            string.Empty,
            student.DateDeleted);
    }
}
