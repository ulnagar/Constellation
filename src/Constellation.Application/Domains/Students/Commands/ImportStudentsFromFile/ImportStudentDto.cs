namespace Constellation.Application.Domains.Students.Commands.ImportStudentsFromFile;

public sealed record ImportStudentDto(
    int RowNumber,
    string StudentReferenceNumber,
    string FirstName,
    string LastName,
    string EmailAddress,
    string Gender,
    string Grade,
    string School);