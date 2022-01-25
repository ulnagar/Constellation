using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Templates.Views.Emails.RollMarking
{
    public class DailyReportEmailViewModel : EmailLayoutBaseViewModel
    {
        public ICollection<string> UnsubmittedRolls { get; set; }
    }
}
