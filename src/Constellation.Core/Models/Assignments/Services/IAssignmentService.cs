namespace Constellation.Core.Models.Assignments.Services;

using Canvas.Models;
using Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAssignmentService
{
    Task<Result> UploadSubmissionToCanvas(CanvasAssignment assignment, CanvasAssignmentSubmission submission, List<CanvasCourseCode> canvasCourseId, CancellationToken cancellationToken = default);
}