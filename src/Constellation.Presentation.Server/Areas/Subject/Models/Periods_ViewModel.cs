using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Subject.Models
{
    public class Periods_ViewModel : BaseViewModel
    {
        public ICollection<TimetablePeriod> Periods { get; set; }

        public Periods_ViewModel()
        {
            Periods = new List<TimetablePeriod>();
        }
    }
}