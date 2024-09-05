namespace Constellation.Core.Tests.Unit.Models.Reports;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Reports;
using Core.Models.Students.Identifiers;

public class AcademicReportTests
{
    [Fact]
    public void Create_ShouldRaiseDomainEvent_WhenReportIsCreated()
    {
        // Arrange
        var sut = AcademicReport.Create(
            new AcademicReportId(),
            new StudentId(),
            "1234",
            "2023",
            "Year 7, Semester 1, 2023");

        // Act
        var result = sut.GetDomainEvents();

        // Assert
        result.Should().HaveCount(1);
        result.First().Should().BeOfType<AcademicReportCreatedDomainEvent>();
    }
}
