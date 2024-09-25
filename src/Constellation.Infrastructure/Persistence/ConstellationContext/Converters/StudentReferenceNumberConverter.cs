namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Core.Models.Students.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal sealed class StudentReferenceNumberConverter : ValueConverter<StudentReferenceNumber, string?>
{
    public StudentReferenceNumberConverter()
        : base(
            srn => StudentReferenceNumberToString(srn),
            value => StringToStudentReferenceNumber(value),
            new ConverterMappingHints())
    { }

    private static string? StudentReferenceNumberToString(StudentReferenceNumber number) =>
        number == StudentReferenceNumber.Empty ? null : number.Number;

    private static StudentReferenceNumber StringToStudentReferenceNumber(string? value) =>
        value == null ? StudentReferenceNumber.Empty : StudentReferenceNumber.FromValue(value);

    public override bool ConvertsNulls => true;
}