namespace Constellation.Core.Models.Assignments.Services;

using System.Threading;
using System.Threading.Tasks;

public interface IAssignmentService
{
    Task<bool> UploadSubmissionToCanvas(CanvasAssignment assignment, CanvasAssignmentSubmission submission, string canvasCourseId, CancellationToken cancellationToken = default);
}