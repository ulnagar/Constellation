namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Application.Common.PresentationModels;
using Constellation.Application.Courses.GetCourseSummary;
using Constellation.Application.Courses.Models;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.AddResourceToOffering;
using Constellation.Application.Offerings.GetOfferingDetails;
using Constellation.Application.Rooms.CreateRoom;
using Constellation.Application.Rooms.GetAllRooms;
using Constellation.Application.Rooms.GetRoomById;
using Constellation.Application.Rooms.Models;
using Constellation.Application.Teams.GetAllTeams;
using Constellation.Application.Teams.GetTeamByName;
using Constellation.Application.Teams.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class ResourceModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public ResourceModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public Phase CurrentStep { get; set; }

    [BindProperty]
    public List<Phase> PreviousSteps { get; set; } = new();

    [BindProperty]
    public string Type { get; set; }
    [BindProperty]
    public string ResourceId { get; set; }
    [BindProperty]
    public string Name { get; set; }
    [BindProperty]
    public bool CreateNew { get; set; }
    public string ResourceName { get; set; }

    public List<RoomResponse> Rooms { get; set; } = new();
    public List<TeamResource> Teams { get; set; } = new();

    public async Task OnGet()
    {
        CurrentStep = Phase.StartEntry;
    }

    public async Task<IActionResult> OnPost()
    {
        if (CurrentStep == Phase.StartEntry)
        {
            if (Type == ResourceType.AdobeConnectRoom.Value)
                CurrentStep = Phase.AdobeConnectRoomSelection;

            if (Type == ResourceType.MicrosoftTeam.Value)
                CurrentStep = Phase.MicrosoftTeamsSelection;

            if (Type == ResourceType.CanvasCourse.Value)
                CurrentStep = Phase.CanvasCourseSelection;

            PreviousSteps.Add(Phase.StartEntry);
        }

        if (CurrentStep == Phase.AdobeConnectRoomSelection || PreviousSteps.Contains(Phase.AdobeConnectRoomSelection))
        {
            Result<List<RoomResponse>> roomRequest = await _mediator.Send(new GetAllRoomsQuery());

            if (roomRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    roomRequest.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

                return Page();
            }

            Rooms = roomRequest.Value;

            if (ResourceId is not null)
            {
                RoomResponse selectedRoom = Rooms.FirstOrDefault(room => room.ScoId == ResourceId);

                if (selectedRoom is not null)
                {
                    ResourceName = selectedRoom.Name;
                }
            }

            if (CurrentStep == Phase.AdobeConnectRoomSelection && (!string.IsNullOrWhiteSpace(ResourceId) || CreateNew))
            {
                PreviousSteps.Add(CurrentStep);
                CurrentStep = Phase.DataEntry;
            }
        }

        if (CurrentStep == Phase.MicrosoftTeamsSelection || PreviousSteps.Contains(Phase.MicrosoftTeamsSelection))
        {
            Result<List<TeamResource>> teamRequest = await _mediator.Send(new GetAllTeamsQuery());

            if (teamRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    teamRequest.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

                return Page();
            }

            Teams = teamRequest.Value;

            if (!string.IsNullOrWhiteSpace(ResourceId))
            {
                ResourceName = ResourceId;
            }

            if (CurrentStep == Phase.MicrosoftTeamsSelection && !string.IsNullOrWhiteSpace(ResourceId))
            {
                PreviousSteps.Add(CurrentStep);
                CurrentStep = Phase.DataEntry;
            }
        }

        if (CurrentStep == Phase.CanvasCourseSelection && string.IsNullOrWhiteSpace(ResourceId))
        {
            OfferingId offeringId = OfferingId.FromValue(Id);

            Result<OfferingDetailsResponse> offeringRequest = await _mediator.Send(new GetOfferingDetailsQuery(offeringId));

            if (offeringRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    offeringRequest.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

                return Page();
            }

            Result<CourseSummaryResponse> courseRequest = await _mediator.Send(new GetCourseSummaryQuery(offeringRequest.Value.CourseId));

            if (courseRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    courseRequest.Error,
                    _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id }));

                return Page();
            }

            string Year = offeringRequest.Value.EndDate.Year.ToString();
            string Grade = courseRequest.Value.Grade.ToString()[1..].PadLeft(2, '0');
            string Code = (string.IsNullOrWhiteSpace(courseRequest.Value.Code) ? "XXX" : courseRequest.Value.Code);

            ResourceId = $"{Year}-{Grade}{Code}";
        }
        else if (CurrentStep == Phase.CanvasCourseSelection)
        {
            PreviousSteps.Add(CurrentStep);
            CurrentStep = Phase.DataEntry;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostFinalSubmit()
    {
        // Send resource creation command
        OfferingId offeringId = OfferingId.FromValue(Id);
        ResourceType resourceType = ResourceType.FromValue(Type);

        if (CreateNew)
        {
            // Create the new item
            // Create the resource record

            if (resourceType == ResourceType.AdobeConnectRoom)
            {
                Result<RoomResponse> roomRequest = await _mediator.Send(new CreateRoomCommand(offeringId));

                if (roomRequest.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        roomRequest.Error,
                        _linkGenerator.GetPathByPage("/Subject/Offerings/Resource", values: new { area = "Staff", Id = Id }));

                    return Page();
                }

                Result request = await _mediator.Send(new AddResourceToOfferingCommand(offeringId, resourceType, roomRequest.Value.Name, roomRequest.Value.UrlPath, roomRequest.Value.ScoId));

                if (request.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        request.Error,
                        _linkGenerator.GetPathByPage("/Subject/Offerings/Resource", values: new { area = "Staff", Id = Id }));

                    return Page();
                }
            }
        }

        if (!CreateNew)
        {
            if (resourceType == ResourceType.AdobeConnectRoom)
            {
                Result<RoomResponse> roomRequest = await _mediator.Send(new GetRoomByIdQuery(ResourceId));

                if (roomRequest.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        roomRequest.Error,
                        _linkGenerator.GetPathByPage("/Subject/Offerings/Resource", values: new { area = "Staff", Id = Id }));

                    return Page();
                }

                Result request = await _mediator.Send(new AddResourceToOfferingCommand(offeringId, resourceType, Name, roomRequest.Value.UrlPath, ResourceId));

                if (request.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        request.Error,
                        _linkGenerator.GetPathByPage("/Subject/Offerings/Resource", values: new { area = "Staff", Id = Id }));

                    return Page();
                }
            }

            if (resourceType == ResourceType.MicrosoftTeam)
            {
                Result<TeamResource> teamRequest = await _mediator.Send(new GetTeamByNameQuery(ResourceId));

                if (teamRequest.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        teamRequest.Error,
                        _linkGenerator.GetPathByPage("/Subject/Offerings/Resource", values: new { area = "Staff", Id = Id }));

                    return Page();
                }

                Result request = await _mediator.Send(new AddResourceToOfferingCommand(offeringId, resourceType, Name, teamRequest.Value.Link, ResourceId));

                if (request.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        request.Error,
                        _linkGenerator.GetPathByPage("/Subject/Offerings/Resource", values: new { area = "Staff", Id = Id }));

                    return Page();
                }
            }

            if (resourceType == ResourceType.CanvasCourse)
            {
                Result request = await _mediator.Send(new AddResourceToOfferingCommand(offeringId, resourceType, Name, string.Empty, ResourceId));

                if (request.IsFailure)
                {
                    ModalContent = new ErrorDisplay(
                        request.Error,
                        _linkGenerator.GetPathByPage("/Subject/Offerings/Resource", values: new { area = "Staff", Id = Id }));

                    return Page();
                }
            }
        }

        return RedirectToPage("/Subject/Offerings/Details", new { area = "Staff", Id = Id });
    }

    public enum Phase
    {
        StartEntry,
        AdobeConnectRoomSelection,
        MicrosoftTeamsSelection,
        CanvasCourseSelection,
        DataEntry
    }
}
