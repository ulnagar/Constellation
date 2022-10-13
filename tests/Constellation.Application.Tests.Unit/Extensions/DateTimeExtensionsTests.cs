using Constellation.Application.Extensions;

namespace Constellation.Application.Tests.Unit.Extensions;

public class DateTimeExtensionsTests
{

    [Theory]
    [InlineData(2022, 1, 1, 0, 0, 0, 0)]
    [InlineData(2022, 1, 1, 0, 0, 10, 10)]
    [InlineData(2022, 5, 1, 6, 31, 0, 10391460)]
    [InlineData(2022, 9, 11, 14, 12, 53, 21910373)]
    [InlineData(2022, 12, 31, 23, 59, 59, 31535999)]
    public void SecondsSinceYearStart_ShouldReturnValue_WithProvidedDates(int year, int month, int day, int hour, int min, int sec, int expected)
    {
		// Arrange
		var sut = new DateTime(year, month, day, hour, min, sec);

        // Act
        var result = sut.SecondsSinceYearStart();

		// Assert
		result.Should().Be(expected);
	}

    [Theory]
    [InlineData(1980, 9, 11, 7, 0, 0, 337467600)]
    [InlineData(1999, 12, 31, 23, 59, 59, 946645199)]
    [InlineData(2015, 1, 27, 8, 55, 0, 1422309300)]
    [InlineData(2022, 10, 13, 11, 0, 0, 1665619200)]
    public void GetUnixEpoch_ShouldReturnValue_WithProvidedDates(int year, int month, int day, int hour, int min, int sec, int expected)
    {
        // Arrange
        var sut = new DateTime(year, month, day, hour, min, sec);

        // Act
        var result = sut.GetUnixEpoch();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetDayNumber_ShouldReturnZero_WhenProvidedADateThatIsAWeekend()
    {
        // Arrange
        var sut = new DateTime(2018, 1, 27); // Saturday, 27th January, 2018

        // Act
        var result = sut.GetDayNumber();

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData(2015, 1, 26, 1)] // Monday Week A
    [InlineData(2015, 1, 27, 2)] // Tuesday Week A
    [InlineData(2015, 1, 28, 3)] // Wednesday Week A
    [InlineData(2015, 1, 29, 4)] // Thursday Week A
    [InlineData(2015, 1, 30, 5)] // Friday Week A
    [InlineData(2015, 2, 2, 6)]  // Monday Week B
    [InlineData(2015, 2, 3, 7)]  // Tuesday Week B
    [InlineData(2015, 2, 4, 8)]  // Wednesday Week B
    [InlineData(2015, 2, 5, 9)]  // Thursday Week B
    [InlineData(2015, 2, 6, 10)] // Friday Week B
    public void GetDayNumber_ShouldReturnValue_WithProvidedDates(int year, int month, int day, int expected)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var result = date.GetDayNumber();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void VerifyStartOfFortnight_ShouldReturnMondayWeekA_WhenGivenDateInFortnight()
    {
        // Arrange
        var sut = new DateTime(2015, 2, 2);

        // Act
        var result = sut.VerifyStartOfFortnight();

        // Assert
        result.Should().Be(new DateTime(2015, 1, 26));
    }
}
