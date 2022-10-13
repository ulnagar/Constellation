using Constellation.Application.DTOs;
using Constellation.Application.Features.ShortTerm.Covers.Commands;
using Constellation.Application.Interfaces.Providers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.Areas.ShortTerm.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
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
        private readonly IMediator _mediator;

        public CoversController(IUnitOfWork unitOfWork, ICoverService coverService, IMediator mediator)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _coverService = coverService;
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Upcoming");
        }

        public async Task<IActionResult> All()
        {
            var covers = await _unitOfWork.Covers.ForListAsync(cover => true);

            var viewModel = await CreateViewModel<Covers_ViewModel>();
            viewModel.Covers = covers.Select(cover => Covers_ViewModel.CoverDto.ConvertFromCover(cover)).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Upcoming()
        {
            var covers = await _unitOfWork.Covers.ForListAsync(cover => cover.EndDate >= DateTime.Today && !cover.IsDeleted);

            var viewModel = await CreateViewModel<Covers_ViewModel>();
            viewModel.Covers = covers.Select(cover => Covers_ViewModel.CoverDto.ConvertFromCover(cover)).ToList();

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

            classes = classes.OrderBy(course => course.Name).ToList();
            teachers = teachers.OrderBy(teacher => teacher.LastName).ToList();
            casuals = casuals.OrderBy(casual => casual.LastName).ToList();

            var userList = teachers.Select(teacher => new { Id = teacher.StaffId, Name = teacher.DisplayName, Sort = teacher.LastName, Group = "Teachers" }).ToList();
            userList.AddRange(casuals.Select(casual => new { Id = casual.Id.ToString(), Name = casual.DisplayName, Sort = casual.LastName, Group = "Casuals" }));

            var viewModel = await CreateViewModel<Covers_CreateViewModel>();
            viewModel.UserList = new SelectList(userList.OrderBy(user => user.Sort), "Id", "Name", null, "Group");
            viewModel.ClassList = new SelectList(classes, "Id", "Name");
            viewModel.TeacherList = new SelectList(teachers.OrderBy(entry => entry.LastName), "StaffId", "DisplayName");
            viewModel.MultiClassList = new MultiSelectList(classes.OrderBy(entry => entry.Name), "Id", "Name");

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

                classes = classes.OrderBy(course => course.Name).ToList();
                teachers = teachers.OrderBy(teacher => teacher.LastName).ToList();
                casuals = casuals.OrderBy(casual => casual.LastName).ToList();

                var userList = teachers.Select(teacher => new { Id = teacher.StaffId, Name = teacher.DisplayName, Group = "Teachers" }).ToList();
                userList.AddRange(casuals.Select(casuals => new { Id = casuals.Id.ToString(), Name = casuals.DisplayName, Group = "Casuals" }));

                viewModel.UserList = new SelectList(userList, "Id", "Name", null, "Group");
                viewModel.ClassList = new SelectList(classes, "Id", "Name");
                viewModel.TeacherList = new SelectList(teachers, "StaffId", "DisplayName");
                viewModel.MultiClassList = new MultiSelectList(classes, "Id", "Name");

                return View(viewModel);
            }

            await _mediator.Send(new CreateNewCoverCommand { CoverDto = viewModel.Cover });
            
            await _unitOfWork.CompleteAsync();
            return RedirectToAction("Index");
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.CoverEditor)]
        public async Task<IActionResult> Update(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("Index");
            }

            var casualCover = await _unitOfWork.CasualClassCovers.ForEditAsync(id);
            var teacherCover = await _unitOfWork.TeacherClassCovers.ForEditAsync(id);

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
            await _mediator.Send(new CancelCoverCommand { CoverId = id });

            return RedirectToAction("Index");
        }
    }
}