namespace Constellation.Core.Models.EnrolmentContext.Application;

using Enums;

public sealed record CourseSelection(
    EnrolmentCourse Course, 
    string Line, 
    CourseSelectionStatus Status = CourseSelectionStatus.Pending);