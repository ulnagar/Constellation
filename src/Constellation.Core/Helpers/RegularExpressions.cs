namespace Constellation.Core.Helpers;

using System.Text.RegularExpressions;

public static partial class RegularExpressions
{
    /// <summary>
    /// For matching and extracting the message ID from email server responses, which typically come in the format of '2.6.0 <message-id>'.
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\d\.\d\.\d\s+<([^>]+)>")]
    public static partial Regex EmailServerMessageId();

    /// <summary>
    /// For matching tutorial team names from Microsoft Teams, in the format of xxTyyXa, where xx is the grade, yy is the student initials, and a is the sequence.
    /// </summary>
    [GeneratedRegex(@"\d{2}T[a-zA-Z]{2}X\d")]
    public static partial Regex TutorialName();

    /// <summary>
    /// Creates a regular expression that matches commas in a CSV row, ignoring commas inside quoted fields.
    /// </summary>
    /// <remarks>This regular expression is designed for parsing comma-separated values (CSV) where fields may
    /// be enclosed in double quotes. It ensures that commas within quoted fields are not treated as delimiters. The
    /// returned Regex is suitable for use with the Regex.Split method to separate fields in a single CSV row.</remarks>
    /// <returns>A Regex instance that can be used to split a CSV row into fields, correctly handling quoted values.</returns>
    [GeneratedRegex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))")]
    public static partial Regex CommaSeparatedValueRow();

    /// <summary>
    /// Returns a regular expression that matches commas in a CSV row, excluding commas within quoted fields.
    /// </summary>
    /// <remarks>Use this regular expression to split CSV rows into fields while correctly handling quoted
    /// values that may contain commas. This is useful for parsing standard CSV formats where fields can be enclosed in
    /// double quotes.</remarks>
    [GeneratedRegex("[,]{1}(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))")]
    public static partial Regex CommaSeparatedValueRowWithQuotedContent();

    /// <summary>
    /// Match an internal employee ID, either Casual or Permanent.
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{6,9}$")]
    public static partial Regex EmployeeId();

    /// <summary>
    /// Match an internal Student Reference Number (SRN)
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{9}$")]
    public static partial Regex StudentReferenceNumber();

    /// <summary>
    /// Provides a compiled regular expression that matches valid email address formats.
    /// </summary>
    [GeneratedRegex(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,6}|[0-9]{1,3})(\]?)$")]
    public static partial Regex EmailAddress();

    /// <summary>
    /// Matches a phone number in the format of an optional country code (+61), followed by an optional area code (with or without parentheses), and then the local number, which can include spaces or dashes for separation. The area code must start with 0 and be followed by a digit between 2 and 8, and the local number must consist of 8 digits, allowing for optional separators.
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^(?:\+?(61))? ?(?:\((?=.*\)))?(0?[2-57-8])\)? ?(\d\d(?:[- ](?=\d{3})|(?!\d\d[- ]?\d[- ]))\d\d[- ]?\d[- ]?\d{3})$")]
    public static partial Regex PhoneNumber();

}
