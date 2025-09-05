namespace Constellation.Application.Domains.MeritAwards.Nominations.Queries.ExportAwardNominations;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Awards.Identifiers;
using System.ComponentModel.DataAnnotations;

public sealed record ExportAwardNominationsCommand(
    AwardNominationPeriodId PeriodId,
    ExportAwardNominationsCommand.GroupCategory Category,
    bool ShowClass,
    bool ShowGrade)
    : ICommand<FileDto>
{
    public enum GroupCategory
    {
        [Display(Name = "None")]
        None,
        [Display(Name = "By School")]
        BySchool,
        [Display(Name = "By Student")]
        ByStudent,
        [Display(Name = "By Subject")]
        BySubject
    }
}