using Constellation.Application.Extensions;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.Areas.Reports.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Reports.Controllers
{
    using Application.Enrolments.GetFTETotalByGrade;
    using Constellation.Core.Models.Offerings.Repositories;
    using Core.Extensions;
    using Core.Shared;

    [Area("Reports")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class StudentsController : BaseController
    {
        private readonly IOfferingRepository _offeringRepository;
        private readonly IMediator _mediator;

        public StudentsController(
            IOfferingRepository offeringRepository,
            IMediator mediator)
            : base(mediator)
        {
            _offeringRepository = offeringRepository;
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await CreateViewModel<BaseViewModel>();

            return View(viewModel);
        }

        public async Task<IActionResult> FTEBreakdown()
        {
            Result<List<GradeFTESummaryResponse>> request = await _mediator.Send(new GetFTETotalByGradeQuery());

            if (request.IsFailure)
            {
                return BadRequest();
            }
            
            Student_FTEBreakdown_ViewModel viewModel = await CreateViewModel<Student_FTEBreakdown_ViewModel>();

            foreach (GradeFTESummaryResponse entry in request.Value)
            {
                viewModel.Grades.Add(new()
                {
                    Grade = entry.Grade.AsName(),
                    MaleEnrolments = entry.MaleEnrolments,
                    MaleEnrolmentFTE = entry.MaleEnrolmentFTE,
                    FemaleEnrolments = entry.FemaleEnrolments,
                    FemaleEnrolmentFTE = entry.FemaleEnrolmentFTE
                });
            }

            viewModel.TotalMaleEnrolments = request.Value.Sum(grade => grade.MaleEnrolments);
            viewModel.TotalMaleEnrolmentFTE = request.Value.Sum(grade => grade.MaleEnrolmentFTE);
            viewModel.TotalFemaleEnrolments = request.Value.Sum(grade => grade.FemaleEnrolments);
            viewModel.TotalFemaleEnrolmentFTE = request.Value.Sum(grade => grade.FemaleEnrolmentFTE);

            viewModel.TotalEnrolments = request.Value.Sum(grade => grade.TotalEnrolments);
            viewModel.TotalEnrolmentFTE = request.Value.Sum(grade => grade.TotalEnrolmentFTE);

            return View(viewModel);
        }

        public async Task<IActionResult> InterviewDetails()
        {
            var offerings = await _offeringRepository.GetAllActive();

            var viewModel = await CreateViewModel<Student_InterviewDetails_ViewModel>();
            viewModel.AllClasses = new SelectList(offerings, "Id", "Name");

            return View(viewModel);
        }
    }
}