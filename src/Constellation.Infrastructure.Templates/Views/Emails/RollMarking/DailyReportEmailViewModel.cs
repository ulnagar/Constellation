using Constellation.Application.DTOs;
using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Templates.Views.Emails.RollMarking
{
    public class DailyReportEmailViewModel : EmailLayoutBaseViewModel
    {
        public List<RollMarkingEmailDto> RollEntries { get; set; } = new();
    }
}
