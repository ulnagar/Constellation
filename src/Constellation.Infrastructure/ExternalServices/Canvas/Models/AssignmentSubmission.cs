namespace Constellation.Infrastructure.ExternalServices.Canvas.Models;

using Newtonsoft.Json;

internal sealed class AssignmentSubmission
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("grade")]
    public string Grade { get; set; }
    [JsonProperty("score")]
    public double? Mark { get; set; }
    [JsonProperty("assignment_id")]
    public int AssignmentId { get; set; }
    [JsonProperty("user_id")]
    public int UserId { get; set; }
    [JsonProperty("workflow_state")]
    public string WorkflowState { get; set; }
    [JsonProperty("rubric_assessment")]
    public Dictionary<string, RubricValue> RubricAssessment { get; set; } = new();
    [JsonProperty("submission_comments")]
    public List<SubmissionComments> Comments { get; set; } = new();

    internal sealed class RubricValue
    {
        [JsonProperty("rating_id")]
        public string RatingId { get; set; }
        [JsonProperty("comments")]
        public string Comments { get; set; }
        [JsonProperty("points")]
        public double? Points { get; set; }
    }

    internal sealed class SubmissionComments
    {
        [JsonProperty("author_name")]
        public string Author { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}