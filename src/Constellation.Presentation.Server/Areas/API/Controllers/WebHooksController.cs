namespace Constellation.Presentation.Server.Areas.API.Controllers;

using Application.Domains.Attendance.CheckIns.Commands.AddCheckInResponse;
using Application.Domains.Attendance.CheckIns.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;

[Route("webhooks")]
[ApiController]
public class WebHooksController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public WebHooksController(
        ISender mediator,
        ILogger logger)
    {
        _mediator = mediator;
        _logger = logger
            .ForContext<WebHooksController>();
    }

    [HttpPost]
    [Route("checkin/s45maths")]
    public async Task<IActionResult> Stage45MathematicsCheckIn([FromBody] object body)
    {
        string stringValue = body.ToString();
        if (stringValue is null)
        {
            _logger
                .ForContext("Received Data", body, true)
                .Warning("Failed to convert WebHook response");

            return Ok();
        }

        FormResponse? response = JsonSerializer.Deserialize<FormResponse>(stringValue);

        if (response is null)
        {
            _logger
                .ForContext("Received Data", stringValue, true)
                .Warning("Failed to convert WebHook response");

            return Ok();
        }

        response.Group = GroupOption.Mathematics;

        await _mediator.Send(new AddCheckInResponseCommand(response));

        return Ok();
    }

    [HttpPost]
    [Route("checkin/s45english")]
    public async Task<IActionResult> Stage45EnglishCheckIn([FromBody] object body)
    {
        string stringValue = body.ToString();
        if (stringValue is null)
        {
            _logger
                .ForContext("Received Data", body, true)
                .Warning("Failed to convert WebHook response");

            return Ok();
        }

        FormResponse? response = JsonSerializer.Deserialize<FormResponse>(stringValue);

        if (response is null)
        {
            _logger
                .ForContext("Received Data", stringValue, true)
                .Warning("Failed to convert WebHook response");

            return Ok();
        }

        response.Group = GroupOption.English;

        await _mediator.Send(new AddCheckInResponseCommand(response));

        return Ok();
    }


    [HttpPost]
    [Route("checkin/s45science")]
    public async Task<IActionResult> Stage45ScienceCheckIn([FromBody] object body)
    {
        string stringValue = body.ToString();
        if (stringValue is null)
        {
            _logger
                .ForContext("Received Data", body, true)
                .Warning("Failed to convert WebHook response");

            return Ok();
        }

        FormResponse? response = JsonSerializer.Deserialize<FormResponse>(stringValue);

        if (response is null)
        {
            _logger
                .ForContext("Received Data", stringValue, true)
                .Warning("Failed to convert WebHook response");

            return Ok();
        }

        response.Group = GroupOption.Science;

        await _mediator.Send(new AddCheckInResponseCommand(response));

        return Ok();
    }


    [HttpPost]
    [Route("checkin/stage3")]
    public async Task<IActionResult> Stage3CheckIn([FromBody] object body)
    {
        string stringValue = body.ToString();
        if (stringValue is null)
        {
            _logger
                .ForContext("Received Data", body, true)
                .Warning("Failed to convert WebHook response");

            return Ok();
        }

        FormResponse? response = JsonSerializer.Deserialize<FormResponse>(stringValue);

        if (response is null)
        {
            _logger
                .ForContext("Received Data", stringValue, true)
                .Warning("Failed to convert WebHook response");

            return Ok();
        }

        response.Group = GroupOption.Stage3;

        await _mediator.Send(new AddCheckInResponseCommand(response));

        return Ok();
    }

    [HttpPost]
    [Route("checkin/stage6")]
    public async Task<IActionResult> Stage6CheckIn([FromBody] object body)
    {
        string stringValue = body.ToString();
        if (stringValue is null)
        {
            _logger
                .ForContext("Received Data", body, true)
                .Warning("Failed to convert WebHook response");

            return Ok();
        }

        FormResponse? response = JsonSerializer.Deserialize<FormResponse>(stringValue);

        if (response is null)
        {
            _logger
                .ForContext("Received Data", stringValue, true)
                .Warning("Failed to convert WebHook response");

            return Ok();
        }

        response.Group = GroupOption.Stage6;

        await _mediator.Send(new AddCheckInResponseCommand(response));

        return Ok();
    }
}
