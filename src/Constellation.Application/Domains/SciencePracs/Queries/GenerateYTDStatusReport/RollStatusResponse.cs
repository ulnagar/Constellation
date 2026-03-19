namespace Constellation.Application.Domains.SciencePracs.Queries.GenerateYTDStatusReport;

using Core.Enums;
using Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record RollStatusResponse(
    SchoolCode SchoolCode,
    string SchoolName,
    List<RollStatusResponse.RollStatusGradeData> GradeData)
{
    public sealed record RollStatusGradeData(
        Grade Grade,
        int SubmittedRolls,
        int TotalRolls,
        bool CurrentStudents);
}