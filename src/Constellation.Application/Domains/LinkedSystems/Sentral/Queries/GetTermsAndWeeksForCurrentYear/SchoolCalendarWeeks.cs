namespace Constellation.Application.Domains.LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;

public sealed record SchoolCalendarWeek(
    string TermGroup,
    DateTime StartDate,
    DateTime EndDate,
    string Description);