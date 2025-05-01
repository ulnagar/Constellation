namespace Constellation.Application.Domains.Enrolments.Queries.GetFTETotalByGrade;

using Constellation.Core.Enums;

public sealed record GradeFTESummaryResponse(
    Grade Grade,
    int MaleEnrolments,
    decimal MaleEnrolmentFTE,
    int FemaleEnrolments,
    decimal FemaleEnrolmentFTE)
{
    public int TotalEnrolments => MaleEnrolments + FemaleEnrolments;
    public decimal TotalEnrolmentFTE => MaleEnrolmentFTE + FemaleEnrolmentFTE;
}
