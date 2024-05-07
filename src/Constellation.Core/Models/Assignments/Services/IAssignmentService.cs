namespace Constellation.Core.Models.Assignments.Services;

using Canvas.Models;
using Shared;
using System.Threading;
using System.Threading.Tasks;

public interface IAssignmentService
{
    Task<Result> UploadSubmissionToCanvas(CanvasAssignment assignment, CanvasAssignmentSubmission submission, CanvasCourseCode canvasCourseId, CancellationToken cancellationToken = default);
}