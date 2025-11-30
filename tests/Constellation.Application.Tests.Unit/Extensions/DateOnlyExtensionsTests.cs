namespace Constellation.Application.Tests.Unit.Extensions;

using Constellation.Application.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DateOnlyExtensionsTests
{
    [Theory]
    [InlineData("2025-11-12", new[] { 1, 2, 4 }, "2025-11-13")]
    [InlineData("2025-11-14", new[] { 1, 2, 4 }, "2025-11-24")]
    [InlineData("2025-11-18", new[] { 1, 2, 4 }, "2025-11-24")]
    [InlineData("2025-11-19", new[] { 6, 7, 9 }, "2025-11-20")]
    [InlineData("2025-11-21", new[] { 6, 7, 9 }, "2025-12-01")]
    [InlineData("2025-11-21", new int[0], "2025-11-21")]
    public void GetFirstDayFromCycleAfterDate_ShouldReturnValue_WithProvidedDates(string date, int[] daysOfCycle, string expectedDate)
    {
        // Arrange
        var sut = DateOnly.Parse(date);
        var scheduledDays = daysOfCycle.ToList();
        var expected = DateOnly.Parse(expectedDate);

        // Act
        var result = sut.GetFirstDayFromCycleAfterDate(scheduledDays);

        // Assert
        result.Should().Be(expected);
    }
}
