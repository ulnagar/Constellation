﻿using Constellation.Application.DTOs;
using Constellation.Application.Features.API.Schools.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Constellation.Presentation.Server.Areas.API.Controllers
{
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
            return await _mediator.Send(new GetSchoolCodeOfAllPartnerSchoolsQuery());
        }

        // GET api/Schools/8912/Graph?day=0
        [Route("{id}/Graph")]
        public async Task<GraphData> GetGraphData(string id, int day)
        {
            return await _mediator.Send(new GetGraphDataForSchoolQuery { SchoolCode = id, Day = day });
        }

    }
}
