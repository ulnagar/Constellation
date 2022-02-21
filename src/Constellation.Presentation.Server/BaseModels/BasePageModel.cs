using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.BaseModels
{
    public class BasePageModel : PageModel, IBaseModel
    {
        public IDictionary<string, int> Classes { get; set; }

        public BasePageModel()
        {
            Classes = new Dictionary<string, int>();
        }

        public async Task GetClasses(IUnitOfWork unitOfWork)
        {
            var username = User.Identity.Name;
            var teacher = await unitOfWork.Staff.FromEmailForExistCheck(username);

            if (teacher != null)
            {
                var entries = unitOfWork.CourseOfferings.AllForTeacher(teacher.StaffId);

                foreach (var entry in entries)
                {
                    Classes.Add(entry.Name, entry.Id);
                }
            }
        }
    }
}
