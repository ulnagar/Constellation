using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Completion
{
    public class DetailsModel : BasePageModel
    {
        private readonly IMediator _mediator;

        public DetailsModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }
        public CompletionRecordDto Record { get; set; }

        public async Task OnGet()
        {
            await GetClasses(_mediator);

            Record = await _mediator.Send(new GetCompletionRecordDetailsQuery { Id = Id });

            //TODO: Check if return value is null, redirect and display error
        }
    }
}
