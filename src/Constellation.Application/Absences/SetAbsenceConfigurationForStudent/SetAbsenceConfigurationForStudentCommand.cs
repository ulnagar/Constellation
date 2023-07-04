namespace Constellation.Application.Absences.SetAbsenceConfigurationForStudent;

using Constellation.Application.Abstractions.Messaging;

public sealed record SetAbsenceConfigurationForStudentCommand(
    string StudentId,
    string SchoolCode,
    int? GradeFilter)
    : ICommand;