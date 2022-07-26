using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Reports.Models.Awards
{
    public class AwardsChangesListViewModel : BaseViewModel
    {
        public List<AwardRecord> Awards { get; set; } = new();

        public bool IsFiltered { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

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
