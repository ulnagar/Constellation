using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Portal.Pages.Lessons
{
    public class RollModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILessonService _lessonService;

        public RollModel(IUnitOfWork unitOfWork, ILessonService lessonService)
        {
            _unitOfWork = unitOfWork;
            _lessonService = lessonService;
        }

        [BindProperty]
        public LessonRollDto Roll { get; set; }

        public async Task OnGet(Guid? id)
        {
            var username = User.Identity.Name;
            var teacher = await _unitOfWork.SchoolContacts.FromEmailForExistCheck(username);

            if (teacher == null)
                RedirectToPage("/Index", new { area = "" });

            if (!id.HasValue)
                RedirectToPage("/Lessons/Teacher", new { area = "Portal" });

            var roll = await _unitOfWork.Lessons.GetRollForPortal(id.Value);

            Roll = LessonRollDto.ConvertFromRoll(roll);

            foreach (var attend in roll.Attendance)
                Roll.Attendance.Add(LessonRollDto.StudentRollDto.ConvertFromRollAttendance(attend));
        }

        public async Task<IActionResult> OnPost()
        {
            if (Roll.LessonDate > DateTime.Today)
                ModelState.AddModelError("LessonDate", "Cannot deliver a lesson in the future!");

            if (Roll.Attendance.All(attend => !attend.Present) && string.IsNullOrWhiteSpace(Roll.Comment))
                ModelState.AddModelError("Attendance", "Cannot submit a roll without any students present unless you provide a comment!");

            if (!ModelState.IsValid)
                return Page();

            var username = User.Identity.Name;
            var teacher = await _unitOfWork.SchoolContacts.FromEmailForExistCheck(username);

            await _lessonService.SubmitLessonRoll(Roll, teacher);

            return RedirectToPage("/Lessons/Teacher", new { area = "Portal" });
        }
    }
}
