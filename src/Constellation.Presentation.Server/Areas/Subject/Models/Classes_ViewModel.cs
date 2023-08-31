using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Subject.Models
{
    public class Classes_ViewModel : BaseViewModel
    {
        public Classes_ViewModel()
        {
            Offerings = new List<OfferingDto>();
        }

        public ICollection<OfferingDto> Offerings { get; set; }

        public IDictionary<Guid, string> FacultyList { get; set; } = new Dictionary<Guid, string>();

        public class OfferingDto
        {
            public OfferingId Id { get; set; }
            public string Name { get; set; }
            public string CourseName { get; set; }
            public DateTime EndDate { get; set; }
            public ICollection<string> Teachers { get; set; }
            public int MinPerFN { get; set; }

            public static OfferingDto ConvertFromOffering(Offering offering)
            {
                var viewModel = new OfferingDto
                {
                    Id = offering.Id,
                    Name = offering.Name,
                    CourseName = offering.Course.Name,
                    EndDate = offering.EndDate,
                    Teachers = offering.Sessions.Where(session => !session.IsDeleted).Select(session => session.Teacher.DisplayName).Distinct().ToList()
                };

                viewModel.MinPerFN = offering.Sessions.Where(session => !session.IsDeleted).Sum(session => session.Period.EndTime.Subtract(session.Period.StartTime).Minutes);
                viewModel.MinPerFN += offering.Sessions.Where(session => !session.IsDeleted).Sum(session => session.Period.EndTime.Subtract(session.Period.StartTime).Hours * 60);

                return viewModel;
            }
        }
    }
}