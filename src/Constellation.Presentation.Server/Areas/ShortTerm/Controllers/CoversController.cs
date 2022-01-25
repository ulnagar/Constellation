using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.Areas.ShortTerm.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.ShortTerm.Controllers
{
    [Area("ShortTerm")]
    [Roles(AuthRoles.Admin, AuthRoles.CoverEditor, AuthRoles.Editor, AuthRoles.User)]
    public class CoversController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICoverService _coverService;

        public CoversController(IUnitOfWork unitOfWork, ICoverService coverService)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _coverService = coverService;
        }

        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Upcoming");
        }

        public async Task<IActionResult> All()
        {
            var covers = await _unitOfWork.Covers.ForListAsync(cover => true);

            var viewModel = await CreateViewModel<Covers_ViewModel>();
            viewModel.Covers = covers.Select(Covers_ViewModel.CoverDto.ConvertFromCover).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Upcoming()
        {
            var covers = await _unitOfWork.Covers.ForListAsync(cover => cover.EndDate > DateTime.Now);

            var viewModel = await CreateViewModel<Covers_ViewModel>();
            viewModel.Covers = covers.Select(Covers_ViewModel.CoverDto.ConvertFromCover).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("Index");
            }

            var cover = await _unitOfWork.Covers.ForDetailsDisplayAsync(id);

            var viewModel = await CreateViewModel<Covers_DetailsViewModel>();
            viewModel.Cover = Covers_DetailsViewModel.CoverDto.ConvertFromCover(cover);

            return View(viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.CoverEditor)]
        public async Task<IActionResult> Create()
        {
            var teachers = await _unitOfWork.Staff.ForSelectionAsync();
            var casuals = await _unitOfWork.Casuals.ForSelectionAsync();
            var classes = await _unitOfWork.CourseOfferings.ForSelectionAsync();

            var userList = teachers.Select(teacher => new { Id = teacher.StaffId, Name = teacher.DisplayName, Group = "Teachers" }).ToList();
            userList.AddRange(casuals.Select(casuals => new { Id = casuals.Id.ToString(), Name = casuals.DisplayName, Group = "Casuals" }));

            var viewModel = await CreateViewModel<Covers_CreateViewModel>();
            viewModel.UserList = new SelectList(userList, "Id", "Name", null, "Group");
            viewModel.ClassList = new SelectList(classes, "Id", "Name");
            viewModel.TeacherList = new SelectList(teachers, "StaffId", "DisplayName");
            viewModel.MultiClassList = new MultiSelectList(classes, "Id", "Name");

            return View(viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.CoverEditor)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Covers_CreateViewModel viewModel)
        {
            // Update view model here so we have access to the AppSettings object later
            await UpdateViewModel(viewModel);

            if (viewModel.Cover.TeacherId == null && viewModel.Cover.ClassId == null && !viewModel.Cover.SelectedClasses.Any())
            {
                ModelState.AddModelError("TeacherId", $"You must specify either a teacher, a class, or a group of classes for the cover");
            }

            if (viewModel.Cover.StartDate.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("StartDate", $"You cannot create a cover to start in the past");
            }

            if (viewModel.Cover.EndDate.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("EndDate", $"You cannot create a cover to end in the past");
            }

            if (viewModel.Cover.StartDate.Date > viewModel.Cover.EndDate.Date)
            {
                ModelState.AddModelError("EndDate", $"You cannot end a cover before it begins");
            }

            if (!ModelState.IsValid)
            {
                var teachers = await _unitOfWork.Staff.ForSelectionAsync();
                var casuals = await _unitOfWork.Casuals.ForSelectionAsync();
                var classes = await _unitOfWork.CourseOfferings.ForSelectionAsync();

                var userList = teachers.Select(teacher => new { Id = teacher.StaffId, Name = teacher.DisplayName, Group = "Teachers" }).ToList();
                userList.AddRange(casuals.Select(casuals => new { Id = casuals.Id.ToString(), Name = casuals.DisplayName, Group = "Casuals" }));

                viewModel.UserList = new SelectList(userList, "Id", "Name", null, "Group");
                viewModel.ClassList = new SelectList(classes, "Id", "Name");
                viewModel.TeacherList = new SelectList(teachers, "StaffId", "DisplayName");
                viewModel.MultiClassList = new MultiSelectList(classes, "Id", "Name");

                return View(viewModel);
            }

            var result = await _coverService.BulkCreateCovers(viewModel.Cover);

            if (result.Success)
            {
                await _unitOfWork.CompleteAsync();
                return RedirectToAction("Index");
            } else
            {
                var teachers = await _unitOfWork.Staff.ForSelectionAsync();
                var casuals = await _unitOfWork.Casuals.ForSelectionAsync();
                var classes = await _unitOfWork.CourseOfferings.ForSelectionAsync();

                var userList = teachers.Select(teacher => new { Id = teacher.StaffId, Name = teacher.DisplayName, Group = "Teachers" }).ToList();
                userList.AddRange(casuals.Select(casuals => new { Id = casuals.Id.ToString(), Name = casuals.DisplayName, Group = "Casuals" }));

                viewModel.UserList = new SelectList(userList, "Id", "Name", null, "Group");
                viewModel.ClassList = new SelectList(classes, "Id", "Name");
                viewModel.TeacherList = new SelectList(teachers, "StaffId", "DisplayName");
                viewModel.MultiClassList = new MultiSelectList(classes, "Id", "Name");

                return View(viewModel);
            }
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.CoverEditor)]
        public async Task<IActionResult> Update(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("Index");
            }

            var casualCover = _unitOfWork.CasualClassCovers.WithDetails(id);
            var teacherCover = _unitOfWork.TeacherClassCovers.WithDetails(id);

            if (casualCover == null && teacherCover == null)
            {
                return RedirectToAction("Index");
            }

            if (casualCover == null && teacherCover != null)
            {
                var viewModel = await CreateViewModel<Covers_UpdateViewModel>();
                Covers_UpdateViewModel.GetFromTeacherCover(teacherCover, viewModel);

                return View(viewModel);
            }

            if (teacherCover == null && casualCover != null)
            {
                var viewModel = await CreateViewModel<Covers_UpdateViewModel>();
                Covers_UpdateViewModel.GetFromCasualCover(casualCover, viewModel);

                return View(viewModel);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.CoverEditor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Covers_UpdateViewModel viewModel)
        {
            await UpdateViewModel(viewModel);

            if (viewModel.EndDate.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("EndDate", "You cannot edit a cover to end in the past");
            }

            if (viewModel.StartDate.Date > viewModel.EndDate.Date)
            {
                ModelState.AddModelError("EndDate", "You cannot end a cover before it begins");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (viewModel.CoverType == "Casual")
            {
                var cover = await _unitOfWork.CasualClassCovers.ForEditAsync(viewModel.Id);

                var coverResource = new CasualCoverDto
                {
                    Id = cover.Id,
                    CasualId = cover.CasualId,
                    OfferingId = cover.OfferingId,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate
                };

                await _coverService.UpdateCasualCover(coverResource);

                await _unitOfWork.CompleteAsync();
            }

            if (viewModel.CoverType == "Teacher")
            {

                var cover = await _unitOfWork.TeacherClassCovers.ForEditAsync(viewModel.Id);

                var coverResource = new TeacherCoverDto
                {
                    Id = cover.Id,
                    StaffId = cover.StaffId,
                    OfferingId = cover.OfferingId,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate
                };

                await _coverService.UpdateTeacherCover(coverResource);

                await _unitOfWork.CompleteAsync();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Cancel(int id)
        {
            var type = await _unitOfWork.Covers.CoverTypeForCancellationAsync(id);

            if (type == "Casual")
            {
                await _coverService.RemoveCasualCover(id);
                await _unitOfWork.CompleteAsync();

                return RedirectToAction("Index");
            }

            if (type == "Teacher")
            {
                await _coverService.RemoveTeacherCover(id);
                await _unitOfWork.CompleteAsync();

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }
    }
}