namespace Constellation.Presentation.Server.Areas.Subject.Pages.Offerings;

using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingDetails;
using Constellation.Application.Rooms.GetAllRooms;
using Constellation.Application.Teams.GetAllTeams;
using Constellation.Application.Teams.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class ResourceModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public ResourceModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public Phase CurrentStep { get; set; }
    [BindProperty]
    public List<Phase> PreviousSteps { get; set; } = new();

    [BindProperty]
    public string Type { get; set; }
    [BindProperty]
    public string ScoId { get; set; }
    [BindProperty]
    public string TeamName { get; set; }
    [BindProperty]
    public string CanvasCourse { get; set; }
    [BindProperty]
    public string Name { get; set; }
    [BindProperty]
    public string Link { get; set; }

    public List<RoomResponse> Rooms { get; set; } = new();
    public List<TeamResource> Teams { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        if (CurrentStep == Phase.AdobeConnectRoomSelection)
        {
            Result<List<RoomResponse>> roomRequest = await _mediator.Send(new GetAllRoomsQuery());

            if (roomRequest.IsFailure)
            {
                Error = new()
                {
                    Error = roomRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
                };

                return;
            }

            Rooms = roomRequest.Value;
        }

        if (CurrentStep == Phase.MicrosoftTeamsSelection)
        {
            Result<List<TeamResource>> teamRequest = await _mediator.Send(new GetAllTeamsQuery());

            if (teamRequest.IsFailure)
            {
                Error = new()
                {
                    Error = teamRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
                };

                return;
            }

            Teams = teamRequest.Value;
        }

        if (CurrentStep == Phase.CanvasCourseSelection)
        {
            OfferingId offeringId = OfferingId.FromValue(Id);

            Result<OfferingDetailsResponse> offeringRequest = await _mediator.Send(new GetOfferingDetailsQuery(offeringId));

            if (offeringRequest.IsFailure)
            {
                Error = new()
                {
                    Error = offeringRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
                };
                return;
            }
            CanvasCourse = $"{offeringRequest.Value.EndDate.Year}-{offeringRequest.Value.Name.Grade}{offeringRequest.Value.Name.Course}";
        }
    }

    public enum Phase
    {
        AdobeConnectRoomSelection,
        MicrosoftTeamsSelection,
        CanvasCourseSelection,
        DataEntry
    }
}
