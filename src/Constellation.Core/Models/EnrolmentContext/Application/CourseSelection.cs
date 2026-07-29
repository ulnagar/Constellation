namespace Constellation.Core.Models.EnrolmentContext.Application;

using Enums;

public sealed record CourseSelection(EnrolmentCourse Course, CourseSelectionStatus Status = CourseSelectionStatus.Pending);