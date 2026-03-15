namespace Constellation.Infrastructure.Templates.Views.Emails.RollMarking;

using Constellation.Application.DTOs;
using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public sealed class DailyReportEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/RollMarking/DailyReportEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    
    public List<RollMarkingEmailDto> RollEntries { get; set; } = [];
}