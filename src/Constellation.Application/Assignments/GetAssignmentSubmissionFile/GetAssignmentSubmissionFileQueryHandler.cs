namespace Constellation.Application.Assignments.GetAssignmentSubmissionFile;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAssignmentSubmissionFileQueryHandler
    : IQueryHandler<GetAssignmentSubmissionFileQuery, FileDto>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IStoredFileRepository _storedFileRepository;

    public GetAssignmentSubmissionFileQueryHandler(
        IAssignmentRepository assignmentRepository,
        IStoredFileRepository storedFileRepository)
    {
        _assignmentRepository = assignmentRepository;
        _storedFileRepository = storedFileRepository;
    }

    public async Task<Result<FileDto>> Handle(GetAssignmentSubmissionFileQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetById(request.AssignmentId, cancellationToken);

        if (assignment is null)
            return Result.Failure<FileDto>(DomainErrors.Assignments.Assignment.NotFound(request.AssignmentId));

        var submission = assignment.Submissions.FirstOrDefault(entry => entry.Id == request.SubmissionId);

        if (submission is null)
            return Result.Failure<FileDto>(DomainErrors.Assignments.Submission.NotFound(request.SubmissionId));

        var file = await _storedFileRepository.GetAssignmentSubmissionByLinkId(submission.Id.ToString(), cancellationToken);

        if (file is null)
            return Result.Failure<FileDto>(DomainErrors.Documents.AssignmentSubmission.NotFound(submission.Id.ToString()));

        var entry = new FileDto
        {
            FileData = file.FileData,
            FileType = file.FileType,
            FileName = file.Name
        };

        return entry;
    }
}
