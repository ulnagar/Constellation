namespace Constellation.Presentation.Server.Areas.Partner.Controllers;

using Application.Students.UpdateStudent;
using Constellation.Application.Enrolments.EnrolStudent;
using Constellation.Application.Enrolments.UnenrolStudent;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
using Core.Models;
using Core.Models.Enrolments;
using Core.Models.Offerings;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Area("Partner")]
[Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
public class StudentsController : Controller
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public StudentsController(
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }


    [Roles(AuthRoles.Admin, AuthRoles.Editor)]
    public async Task<IActionResult> Update(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToPage("/Partner/Students/Index", new { area = "Staff" });
        }

        Student? student = await _unitOfWork.Students.ForEditAsync(id);

        if (student is null)
        {
            return RedirectToPage("/Partner/Students/Index", new { area = "Staff" });
        }

        List<School> schools = (await _unitOfWork.Schools.ForSelectionAsync()).ToList();

        List<SelectListItem> genders = new()
        {
            new() { Text ="Male", Value = "M" },
            new() { Text = "Female", Value = "F" }
        };

        Student_UpdateViewModel viewModel = new()
        { 
            Student = new()
            {
                StudentId = student.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                PortalUsername = student.PortalUsername,
                AdobeConnectPrincipalId = student.AdobeConnectPrincipalId,
                SentralStudentId = student.SentralStudentId,
                EnrolledGrade = student.EnrolledGrade,
                CurrentGrade = student.CurrentGrade,
                Gender = student.Gender,
                SchoolCode = student.SchoolCode
            },
            IsNew = false,
            SchoolList = new(schools, "Code", "Name", student.SchoolCode),
            GenderList = new(genders, "Value", "Text", student.Gender)
        };

        return View(viewModel);
    }

    [HttpPost]
    [Roles(AuthRoles.Admin, AuthRoles.Editor)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Student_UpdateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            List<School> schools = (await _unitOfWork.Schools.ForSelectionAsync()).ToList();
            List<SelectListItem> genders = new()
            {
                new() { Text ="Male", Value = "M" },
                new() { Text = "Female", Value = "F" }
            };

            viewModel.SchoolList = new(schools, "Code", "Name", viewModel.Student.SchoolCode);
            viewModel.GenderList = new(genders, "Value", "Text", viewModel.Student.Gender);

            return View("Update", viewModel);
        }

        if (viewModel.IsNew)
        {
            Student student = Student.Create(
                viewModel.Student.StudentId,
                viewModel.Student.FirstName,
                viewModel.Student.LastName,
                viewModel.Student.PortalUsername,
                viewModel.Student.CurrentGrade,
                viewModel.Student.SchoolCode,
                viewModel.Student.Gender);

            _unitOfWork.Students.Insert(student);
        }
        else
        {
            await _mediator.Send(new UpdateStudentCommand(
                viewModel.Student.StudentId,
                viewModel.Student.FirstName,
                viewModel.Student.LastName,
                viewModel.Student.PortalUsername,
                viewModel.Student.AdobeConnectPrincipalId,
                viewModel.Student.SentralStudentId,
                viewModel.Student.CurrentGrade,
                viewModel.Student.EnrolledGrade,
                viewModel.Student.Gender,
                viewModel.Student.SchoolCode));
        }

        await _unitOfWork.CompleteAsync();

        return RedirectToPage("/Partner/Students/Index", new { area = "Staff" });
    }

    [Roles(AuthRoles.Admin, AuthRoles.Editor)]
    public async Task<IActionResult> Create()
    {
        List<School> schools = (await _unitOfWork.Schools.ForSelectionAsync()).ToList();

        List<SelectListItem> genders = new()
        {
            new() { Text ="Male", Value = "M" },
            new() { Text = "Female", Value = "F" }
        };

        Student_UpdateViewModel viewModel = new()
        {
            IsNew = true,
            SchoolList = new(schools, "Code", "Name"),
            GenderList = new(genders, "Value", "Text")
        };

        return View("Update", viewModel);
    }
}