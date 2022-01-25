using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class School_ViewModel : BaseViewModel
    {
        public School_ViewModel()
        {
            Schools = new List<SchoolDto>();
        }

        public IEnumerable<SchoolDto> Schools { get; set; }

        public class SchoolDto
        {
            [Display(Name=DisplayNameDefaults.SchoolCode)]
            public string SchoolCode { get; set; }
            public string Name { get; set; }
            public string Town { get; set; }
            [Display(Name=DisplayNameDefaults.PhoneNumber)]
            public string PhoneNumber { get; set; }
            [Display(Name=DisplayNameDefaults.EmailAddress)]
            public string EmailAddress { get; set; }

            public static SchoolDto ConvertFromSchool(School school)
            {
                var viewModel = new SchoolDto
                {
                    SchoolCode = school.Code,
                    Name = school.Name,
                    Town = school.Town,
                    PhoneNumber = school.PhoneNumber,
                    EmailAddress = school.EmailAddress
                };

                return viewModel;
            }
        }
    }
}