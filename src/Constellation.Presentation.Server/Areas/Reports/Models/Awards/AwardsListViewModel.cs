using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Reports.Models.Awards
{
    public class AwardsChangesListViewModel : BaseViewModel
    {
        public List<AwardRecord> Awards { get; set; } = new();

        public class AwardRecord
        {
            public string StudentId { get; set; }
            public string StudentName { get; set; }
            public string StudentGrade { get; set; }
            public decimal AwardedAstras { get; set; }
            public decimal AwardedStellars { get; set; }
            public decimal AwardedGalaxies { get; set; }
            public decimal AwardedUniversals { get; set; }
            public decimal CalculatedStellars { get; set; }
            public decimal CalculatedGalaxies { get; set; }
            public decimal CalculatedUniversals { get; set; }
        }
    }
}
