using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.ShortTerm.Models
{
    public class Casual_ViewModel : BaseViewModel
    {
        public Casual_ViewModel()
        {
            Casuals = new List<CasualDto>();
        }

        public IEnumerable<CasualDto> Casuals { get; set; }

        public class CasualDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string SchoolName { get; set; }
            public int FutureCoverCount { get; set; }


            public static CasualDto ConvertFromCasual(Casual casual)
            {
                var viewModel = new CasualDto
                {
                    Id = casual.Id,
                    Name = casual.DisplayName,
                    SchoolName = casual.School.Name,
                    FutureCoverCount = casual.ClassCovers.Count(cover => cover.EndDate >= DateTime.Now)
                };

                return viewModel;
            }
        }
    }
}