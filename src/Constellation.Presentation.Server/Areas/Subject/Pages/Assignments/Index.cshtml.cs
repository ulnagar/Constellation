using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Subject.Pages.Assignments
{
    public class IndexModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ICollection<AssignmentDto> Assignments { get; set; }

        public async Task OnGet()
        {
            await GetClasses(_unitOfWork);

            //Assignments = await _unitOfWork.
        }

        public class AssignmentDto
        {
            public Guid Id { get; set; }
            public string CourseName { get; set; }
            public string Name { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime? LockDate { get; set; }
            public DateTime? UnlockDate { get; set; }
            public int AllowedAttempts { get; set; }

            public static AssignmentDto ConvertFromCanvasAssignment(CanvasAssignment assignment)
            {
                var viewModel = new AssignmentDto
                {
                    Id = assignment.Id,
                    Name = assignment.Name,
                    DueDate = assignment.DueDate,
                    LockDate = assignment.LockDate,
                    UnlockDate = assignment.UnlockDate,
                    AllowedAttempts = assignment.AllowedAttempts
                };

                if (assignment.Course != null)
                {
                    viewModel.CourseName = $"{assignment.Course.Grade} {assignment.Course.Name}";
                } else
                {
                    viewModel.CourseName = "";
                }

                return viewModel;
            }
        }
    }
}
