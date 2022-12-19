namespace Constellation.Application.Helpers;

using System;

public static class MicrosoftTeamsHelper
{
    public static string FormatTeamName(string unit) => $"AC - {DateTime.Today.Year} - {unit}";

}
