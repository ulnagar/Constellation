using Constellation.Application.Features.Partners.Schools.Models;
using Constellation.Application.Features.Partners.Schools.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Constellation.Presentation.Staff.Areas.Partners.Pages
{
    public class SchoolsModel : PageModel
    {
        private readonly IMediator _mediator;

        public SchoolsModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public List<SchoolForList> Schools { get; set; } = new();

        public async Task<IActionResult> OnGet()
        {
            Schools = await _mediator.Send(new GetSchoolsForListQuery { WithStaff = true, WithStudents = true });

            return Page();
        }
    }
}
