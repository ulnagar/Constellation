namespace Constellation.Application.Students.RemoveSchoolEnrolment;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record RemoveSchoolEnrolmentCommand(
    StudentId StudentId,
    SchoolEnrolmentId EnrolmentId)
    : ICommand;
