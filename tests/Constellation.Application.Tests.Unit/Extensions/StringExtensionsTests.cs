using Constellation.Application.Extensions;

namespace Constellation.Application.Tests.Unit.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void RemoveQuotes_ShouldReturnStringWithoutQuotesAtBeginingAndEnd_WhenProvidedWithStringWithQuotes()
    {
		// Arrange
		var sut = "\"This is a string with quotes\"";

		// Act
		var results = sut.RemoveQuotes();

		// Assert
		results.Should().Be("This is a string with quotes");
	}

	[Fact]
	public void RemoveQuotes_ShouldReturnOriginalString_WhenProvidedWithStringWithoutQuotes()
	{
		// Arrange
		var sut = "This is a string without quotes";

		// Act
		var results = sut.RemoveQuotes();

		// Assert
		results.Should().Be(sut);
	}

	[Fact]
	public void RemoveQuotes_ShouldReturnOriginalString_WhenQuotesAreInTheMiddleOfTheString()
	{
        // Arrange
        var sut = "This is a \"string\" without quotes";

        // Act
        var results = sut.RemoveQuotes();

        // Assert
        results.Should().Be(sut);
    }

	[Fact]
	public void RemoveWhitespace_ShouldReturnStringWithoutSpacesAtBeginingAndEnd_WhenProvidedWithStringWithSpaces()
	{
		// Arrange
		var sut = " string ";

		// Act
		var results = sut.RemoveWhitespace();

		// Assert
		results.Should().Be("string");
	}

    [Fact]
    public void RemoveWhitespace_ShouldReturnOriginalString_WhenProvidedWithStringWithoutSpaces()
    {
        // Arrange
        var sut = "string";

        // Act
        var results = sut.RemoveWhitespace();

        // Assert
        results.Should().Be(sut);
    }

    [Fact]
    public void RemoveWhitespace_ShouldReturnOriginalString_WhenSpacesAreInTheMiddleOfTheString()
    {
        // Arrange
        var sut = "This is a string";

        // Act
        var results = sut.RemoveWhitespace();

        // Assert
        results.Should().Be(sut);
    }

    [Fact]
    public void Collapse_ShouldReturnStringWithDelimiter_WhenProvidedAListOfStrings()
    {
		// Arrange
		var sut = new List<string>();
		sut.Add("Item 1");
		sut.Add("Item 2");
		sut.Add("Item 3");

		// Act
		var result = sut.Collapse('|');

		// Assert
		result.Should().Be("Item 1|Item 2|Item 3");
	}

	[Fact]
	public void Collapse_ShouldReturnString_WhenProvidedWithListContainingSingleString()
	{
        // Arrange
        var sut = new List<string>();
        sut.Add("Item 1");

        // Act
        var result = sut.Collapse('|');

        // Assert
        result.Should().Be("Item 1");
    }

	[Fact]
	public void Expand_ShouldReturnListOfStrings_WhenProvidedWithDelimitedString()
	{
		// Arrange
		var sut = "Item 1|Item 2|Item 3";

		// Act
		var result = sut.Expand('|');

		// Assert
		result.Count.Should().Be(3);
		result.Should().Contain("Item 1");
		result.Should().Contain("Item 2");
		result.Should().Contain("Item 3");
	}

    [Fact]
    public void Expand_ShouldReturnListWithoutWhitespaceEntries_WhenProvidedWithDelimitedStringContainingWhitespaceEntries()
    {
        // Arrange
        var sut = "Item 1| |Item 3";

        // Act
        var result = sut.Expand('|');

        // Assert
        result.Count.Should().Be(2);
		result.Should().Contain("Item 1");
		result.Should().Contain("Item 3");
    }

    [Fact]
	public void Expand_ShouldReturnEmptyList_WhenProvidedWithWhitespaceSeparatedByDelimiter()
	{
		// Arrange
		var sut = " | | ";

		// Act
		var result = sut.Expand('|');

		// Assert
		result.Count.Should().Be(0);
	}
}
