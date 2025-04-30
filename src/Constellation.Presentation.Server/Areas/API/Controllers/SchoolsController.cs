using Constellation.Application.DTOs;
using Constellation.Application.Features.API.Schools.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Constellation.Presentation.Server.Areas.API.Controllers
{
    using Application.Schools.GetCurrentPartnerSchoolCodes;
    using Core.Shared;

    [Route("api/v1/Schools")]
    [ApiController]
    public class SchoolsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SchoolsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/Schools/List
        [Route("List")]
        public async Task<IEnumerable<string>> Get()
        {
            Result<List<string>> schoolCodes = await _mediator.Send(new GetCurrentPartnerSchoolCodesQuery());

            if (schoolCodes.IsFailure)
                return new List<string>();

            return schoolCodes.Value;
        }

        // GET api/Schools/8912/Graph?day=0
        [Route("{id}/Graph")]
        public async Task<GraphData> GetGraphData(string id, int day)
        {
            return await _mediator.Send(new GetGraphDataForSchoolQuery { SchoolCode = id, Day = day });
        }

    }
}
