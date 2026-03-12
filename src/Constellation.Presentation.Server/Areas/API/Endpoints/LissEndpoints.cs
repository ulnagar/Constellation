namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Application.Interfaces.Gateways.LissServerGateway;
using Application.Interfaces.Gateways.LissServerGateway.Models;
using Constellation.Application.Interfaces.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

public static class LissEndpoints
{
    public static void MapLissEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/liss/json", HandleLissConnection)
            .WithName("LissIncoming")
            .Accepts<object>("application/json");
    }

    private static async Task<ILissResponse> HandleLissConnection(object body, ILissServerGateway lissServerGateway, IOptions<LissServerGatewayConfiguration> configuration)
    {
        string? stringValue = body?.ToString();
        if (stringValue is null)
            return LissResponseError.NotValid;

        LissCall? callDetails = JsonSerializer.Deserialize<LissCall>(stringValue);

        if (callDetails is null)
            return LissResponseError.NotValid;

        if (LissCallMethod.FromValue(callDetails.Method) == LissCallMethod.Hello)
            return new LissResponse() { Id = callDetails.Id, Result = new LissResponseHello() };

        if (callDetails.Params.Length == 0)
            return LissResponseError.NotValid;

        // Check authorisation
        string? authorisationString = callDetails.Params[0].ToString();
        if (authorisationString is null)
            return LissResponseError.InvalidAuthentication;

        LissCallAuthorisation? authorisation = JsonSerializer.Deserialize<LissCallAuthorisation>(authorisationString);

        if (authorisation is null)
            return LissResponseError.InvalidAuthentication;

        if (authorisation.UserName != configuration.Value.Username && authorisation.Password != configuration.Value.Password)
            return LissResponseError.InvalidAuthentication;

        // Check Method
        LissCallMethod method = LissCallMethod.FromValue(callDetails.Method);

        return method switch
        {
            _ when method == LissCallMethod.PublishStudents => await lissServerGateway.PublishStudents(callDetails.Params),
            _ when method == LissCallMethod.PublishTimetable => await lissServerGateway.PublishTimetable(callDetails.Params),
            _ when method == LissCallMethod.PublishTeachers => await lissServerGateway.PublishTeachers(callDetails.Params),
            _ when method == LissCallMethod.PublishClassMemberships => await lissServerGateway.PublishClassMemberships(callDetails.Params),
            _ when method == LissCallMethod.PublishClasses => await lissServerGateway.PublishClasses(callDetails.Params),
            _ when method == LissCallMethod.PublishDailyData => await lissServerGateway.PublishDailyData(callDetails.Params),
            _ => new LissResponse()
        };
    }
}