namespace Constellation.Application.Features.API.Operations.Queries;

using MediatR;

public sealed class GetTeamIdForOfferingQuery : IRequest<string>
{
    public string ClassName { get; set; }
    public string Year { get; set; }
}