namespace Constellation.Infrastructure.ExternalServices.Canvas.Models;

using Newtonsoft.Json;

internal class CourseListResult
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("sis_course_id")]
    public string SISId { get; set; }

    [JsonProperty("workflow_state")]
    public string WorkflowState { get; set; }
}