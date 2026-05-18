namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetCanvasCoursesAndAssessments;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCanvasCoursesAndAssessmentsQuery()
    : IQuery<List<CanvasCourseWithAssessmentResponse>>;
