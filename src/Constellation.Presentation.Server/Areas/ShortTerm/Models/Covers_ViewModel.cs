using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Providers;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.ShortTerm.Models
{
    public class Covers_ViewModel : BaseViewModel
    {
        public Covers_ViewModel()
        {
            Covers = new List<CoverDto>();
        }

        public ICollection<CoverDto> Covers { get; set; }


        public class CoverDto
        {
            public int Id { get; set; }
            public int OfferingId { get; set; }
            public string OfferingName { get; set; }
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string UserSchool { get; set; }
            public string UserType { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsDeleted { get; set; }
            public bool IsCurrent { get; set; }
            public bool IsFuture { get; set; }

            public static CoverDto ConvertFromCover<T>(T cover, IDateTimeProvider dateTimeProvider) where T : ClassCover
            {
                var viewModel = new CoverDto
                {
                    Id = cover.Id,
                    OfferingId = cover.OfferingId,
                    EndDate = cover.EndDate,
                    StartDate = cover.StartDate,
                    IsCurrent = cover.IsCurrent(dateTimeProvider),
                    IsFuture = cover.IsFuture(dateTimeProvider),
                    IsDeleted = cover.IsDeleted,
                    OfferingName = cover.Offering.Name
                };

                switch (cover)
                {
                    case CasualClassCover casual:
                        viewModel.UserId = casual.CasualId.ToString();
                        viewModel.UserName = casual.Casual.DisplayName;
                        viewModel.UserSchool = casual.Casual.School.Name;
                        viewModel.UserType = "Casual";
                        break;
                    case TeacherClassCover teacher:
                        viewModel.UserId = teacher.StaffId;
                        viewModel.UserName = teacher.Staff.DisplayName;
                        viewModel.UserSchool = teacher.Staff.School.Name;
                        viewModel.UserType = "Teacher";
                        break;
                }

                return viewModel;
            }
        }
    }
}