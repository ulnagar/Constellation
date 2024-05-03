namespace Constellation.Infrastructure.ExternalServices.Canvas.Models;

using Newtonsoft.Json;

internal class GroupMembershipListResult
{
    [JsonProperty("user_id")]
    public int CanvasUserId { get; set; }

    [JsonProperty("workflow_state")]
    public string WorkflowState { get; set; }
}