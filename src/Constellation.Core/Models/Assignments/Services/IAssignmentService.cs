namespace Constellation.Core.Models.Assignments.Services;

using Shared;
using System.Threading;
using System.Threading.Tasks;

public interface IAssignmentService
{
    Task<Result> UploadSubmissionToCanvas(CanvasAssignment assignment, CanvasAssignmentSubmission submission, string canvasCourseId, CancellationToken cancellationToken = default);
}