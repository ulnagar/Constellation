using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Constellation.Presentation.Server.BaseModels
{
    public class BasePageModel : PageModel, IBaseModel
    {
        public IDictionary<string, int> Classes { get; set; }

        public BasePageModel()
        {
            Classes = new Dictionary<string, int>();
        }

        public void GetClasses(IUnitOfWork unitOfWork)
        {
            var user = (ClaimsIdentity)User.Identity;
            var claims = user.Claims;
            var staffId = claims.FirstOrDefault(claim => claim.Type == AppUser.StaffMemberId);

            if (staffId != null && !string.IsNullOrWhiteSpace(staffId.Value))
            {
                var entries = unitOfWork.CourseOfferings.AllForTeacher(staffId.Value);

                foreach (var entry in entries)
                {
                    Classes.Add(entry.Name, entry.Id);
                }
            }
        }
    }
}
