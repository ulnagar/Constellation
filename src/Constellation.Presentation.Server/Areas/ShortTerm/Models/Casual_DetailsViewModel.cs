using Constellation.Core.Models.Casuals;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.ShortTerm.Models
{
    public class Casual_DetailsViewModel : BaseViewModel
    {
        public CasualDto Casual { get; set; }

        public class CasualDto
        {
            public CasualDto()
            {
                Covers = new List<CoverDto>();
            }

            public int Id { get; set; }
            public string Name { get; set; }
            [Display(Name=DisplayNameDefaults.IsDeleted)]
            public bool IsDeleted { get; set; }
            [Display(Name = DisplayNameDefaults.SchoolName)]
            public string SchoolName { get; set; }
            [Display(Name = DisplayNameDefaults.DateEntered)]
            public DateTime? DateEntered { get; set; }
            [Display(Name = DisplayNameDefaults.EmailAddress)]
            public string EmailAddress { get; set; }
            [Display(Name = DisplayNameDefaults.DateDeleted)]
            public DateTime? DateDeleted { get; set; }
            [Display(Name = DisplayNameDefaults.AdobeConnectId)]
            public string AdobeConnectId { get; set; }
            public ICollection<CoverDto> Covers { get; set; }

            public static CasualDto ConvertFromCasual(Casual casual)
            {
                var viewModel = new CasualDto
                {
                    Id = casual.Id,
                    Name = casual.DisplayName,
                    EmailAddress = casual.EmailAddress,
                    SchoolName = casual.School.Name,
                    DateEntered = casual.DateEntered,
                    DateDeleted = casual.DateDeleted,
                    AdobeConnectId = casual.AdobeConnectPrincipalId,
                    IsDeleted = casual.IsDeleted,
                    Covers = casual.ClassCovers.Where(cover => cover.EndDate > DateTime.Now).Select(CoverDto.ConvertFromCover).ToList()
                };

                return viewModel;
            }
        }

        public class CoverDto
        {
            public int Id { get; set; }
            public string OfferingName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; }

            public static CoverDto ConvertFromCover(CasualClassCover cover)
            {
                var viewModel = new CoverDto
                {
                    Id = cover.Id,
                    OfferingName = cover.Offering.Name,
                    StartDate = cover.StartDate,
                    EndDate = cover.EndDate
                };

                if (cover.StartDate <= DateTime.Now && cover.EndDate >= DateTime.Now)
                    viewModel.Status = "cover-current";

                if (cover.StartDate > DateTime.Now)
                    viewModel.Status = "cover-future";

                return viewModel;
            }
        }
    }
}