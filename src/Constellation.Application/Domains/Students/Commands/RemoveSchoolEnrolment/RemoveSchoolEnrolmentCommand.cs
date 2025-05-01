namespace Constellation.Application.Domains.Students.Commands.RemoveSchoolEnrolment;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record RemoveSchoolEnrolmentCommand(
    StudentId StudentId,
    SchoolEnrolmentId EnrolmentId)
    : ICommand;
