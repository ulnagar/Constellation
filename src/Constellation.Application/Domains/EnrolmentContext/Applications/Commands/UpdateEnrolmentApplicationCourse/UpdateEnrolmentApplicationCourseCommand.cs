namespace Constellation.Application.Domains.EnrolmentContext.Applications.Commands.UpdateEnrolmentApplicationCourse;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application.Enums;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

public sealed record UpdateEnrolmentApplicationCourseCommand(
    ApplicationId ApplicationId,
    EnrolmentCourse Course,
    CourseSelectionStatus Status)
    : ICommand;