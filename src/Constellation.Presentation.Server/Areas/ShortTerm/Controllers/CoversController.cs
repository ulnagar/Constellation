namespace Constellation.Presentation.Server.Areas.ShortTerm.Controllers;

using Constellation.Application.Casuals.GetCasualsForSelectionList;
using Constellation.Application.ClassCovers.GetAllCurrentAndFutureCovers;
using Constellation.Application.ClassCovers.GetCoverWithDetails;
using Constellation.Application.ClassCovers.GetFutureCovers;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingsForSelectionList;
using Constellation.Application.Staff.GetForSelectionList;
using Constellation.Presentation.Server.Areas.ShortTerm.Models;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Area("ShortTerm")]
[Authorize(Policy = AuthPolicies.CanViewCovers)]
public class CoversController : BaseController
{
    private readonly IMediator _mediator;

    public CoversController(IMediator mediator)
        : base(mediator)
    {   
        _mediator = mediator;
    }

    [Authorize(Policy = AuthPolicies.CanEditCovers)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var teacherResponse = await _mediator.Send(new GetStaffForSelectionListQuery(), cancellationToken);
        var casualResponse = await _mediator.Send(new GetCasualsForSelectionListQuery(), cancellationToken);

        if (teacherResponse.IsFailure || casualResponse.IsFailure)
            return RedirectToAction("Index");

        var teachers = teacherResponse
            .Value
            .Select(teacher =>
                new {
                    Id = teacher.StaffId,
                    Name = $"{teacher.FirstName} {teacher.LastName}",
                    Sort = teacher.LastName
                })
            .ToList();

        var userList = teacherResponse
            .Value
            .Select(teacher => 
                new { 
                    Id = teacher.StaffId, 
                    Name = $"{teacher.FirstName} {teacher.LastName} (Staff)", 
                    Sort = teacher.LastName, 
                    Group = "Teachers" })
            .ToList();

        userList.AddRange(casualResponse
            .Value
            .Select(casual =>
                new {
                    Id = casual.Id.ToString(),
                    Name = $"{casual.FirstName} {casual.LastName} (Casual)",
                    Sort = casual.LastName,
                    Group = "Casuals" })
            .ToList());

        var classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);

        if (classesResponse.IsFailure)
            return RedirectToAction("Index");

        var classes = classesResponse
            .Value
            .OrderBy(course => course.Name)
            .ToList();

        var viewModel = await CreateViewModel<Covers_CreateViewModel>();
        viewModel.UserList = new SelectList(userList.OrderBy(user => user.Sort), "Id", "Name", null, "Group");
        viewModel.ClassList = new SelectList(classes, "Id", "Name");
        viewModel.TeacherList = new SelectList(teachers.OrderBy(teacher => teacher.Sort), "Id", "Name");
        viewModel.MultiClassList = new MultiSelectList(classes, "Id", "Name");

        return View(viewModel);
    }

    [Authorize(Policy = AuthPolicies.CanEditCovers)]
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

        await _mediator.Send(new CreateCoverCommand { CoverDto = viewModel.Cover });
        
        await _unitOfWork.CompleteAsync();
        return RedirectToAction("Index");
    }

    [Authorize(Policy = AuthPolicies.CanEditCovers)]
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

    [Authorize(Policy = AuthPolicies.CanEditCovers)]
    [HttpPost]
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


}