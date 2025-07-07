namespace Constellation.Presentation.Server.Areas.API.Controllers;

using Application.Interfaces.Configuration;
using Application.Interfaces.Gateways.LissServerGateway;
using Application.Interfaces.Gateways.LissServerGateway.Models;
using Constellation.Infrastructure.ExternalServices.LissServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.Configuration;
using Serilog;
using System.Text.Json;

[Route("liss")]
[ApiController]
public sealed class LissController : ControllerBase
{
    private readonly LissServerGatewayConfiguration _configuration;
    private readonly ILissServerGateway _lissServerGateway;
    private readonly ILogger _logger;

    public LissController(
        IOptions<LissServerGatewayConfiguration> configuration,
        ILissServerGateway lissServerGateway,
        ILogger logger)
    {
        if (!configuration.Value.IsConfigured())
            throw new InvalidConfigurationException("LissServerGatewayConfiguration is invalid!");

        _configuration = configuration.Value;
        _lissServerGateway = lissServerGateway;
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

        if (authorisation.UserName != _configuration.Username && authorisation.Password != _configuration.Password)
            return LissResponseError.InvalidAuthentication;

        // Check Method
        LissCallMethod method = LissCallMethod.FromValue(callDetails.Method);

        return method switch
        {
            _ when method == LissCallMethod.PublishStudents => await _lissServerGateway.PublishStudents(callDetails.Params),
            _ when method == LissCallMethod.PublishTimetable => await _lissServerGateway.PublishTimetable(callDetails.Params),
            _ when method == LissCallMethod.PublishTeachers => await _lissServerGateway.PublishTeachers(callDetails.Params),
            _ when method == LissCallMethod.PublishClassMemberships => await _lissServerGateway.PublishClassMemberships(callDetails.Params),
            _ when method == LissCallMethod.PublishClasses => await _lissServerGateway.PublishClasses(callDetails.Params),
            _ when method == LissCallMethod.PublishDailyData => await _lissServerGateway.PublishDailyData(callDetails.Params),
            _ => new LissResponse()
        };
    }
}
