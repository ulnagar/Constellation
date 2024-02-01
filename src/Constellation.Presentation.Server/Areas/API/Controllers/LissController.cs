namespace Constellation.Presentation.Server.Areas.API.Controllers;

using Constellation.Infrastructure.ExternalServices.LissServer;
using Constellation.Infrastructure.ExternalServices.LissServer.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;

[Route("liss")]
[ApiController]
public sealed class LissController : ControllerBase
{
    private readonly ILissServerService _lissServerService;
    private readonly ILogger _logger;

    public LissController(
        ILissServerService lissServerService,
        ILogger logger)
    {
        _lissServerService = lissServerService;
        _logger = logger.ForContext<LissController>();
    }

    [HttpPost]
    [Route("json")]
    public async Task<ILissResponse> ReceiveJson([FromBody] object body)
    {
        string stringValue = body.ToString();
        if (stringValue is null)
            return LissResponseError.NotValid;

        LissCall callDetails = JsonSerializer.Deserialize<LissCall>(stringValue);

        if (callDetails is null)
            return LissResponseError.NotValid;

        if (LissCallMethod.FromValue(callDetails.Method) == LissCallMethod.Hello)
            return new LissResponse() { Id = callDetails.Id, Result = new LissResponseHello() };

        if (callDetails.Params.Length == 0 || callDetails.Params[0] is null)
            return LissResponseError.NotValid;

        // Check authorisation
        string authorisationString = callDetails.Params[0].ToString();
        if (authorisationString is null)
            return LissResponseError.InvalidAuthentication;
            
        LissCallAuthorisation authorisation = JsonSerializer.Deserialize<LissCallAuthorisation>(authorisationString);

        if (authorisation.UserName != "backup.admin" && authorisation.Password != "8912local")
            return LissResponseError.InvalidAuthentication;

        // Check Method
        LissCallMethod method = LissCallMethod.FromValue(callDetails.Method);

        if (method == LissCallMethod.PublishStudents)
            return await _lissServerService.PublishStudents(callDetails.Params);

        if (method == LissCallMethod.PublishTimetable)
            return await _lissServerService.PublishTimetable(callDetails.Params);

        if (method == LissCallMethod.PublishTeachers)
            return await _lissServerService.PublishTeachers(callDetails.Params);

        if (method == LissCallMethod.PublishClassMemberships)
            return await _lissServerService.PublishClassMemberships(callDetails.Params);

        if (method == LissCallMethod.PublishClasses)
            return await _lissServerService.PublishClasses(callDetails.Params);

        return new LissResponse();
    }
}
