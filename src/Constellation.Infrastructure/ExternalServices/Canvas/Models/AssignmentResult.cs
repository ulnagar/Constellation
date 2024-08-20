namespace Constellation.Infrastructure.ExternalServices.Canvas.Models;

using Newtonsoft.Json;
using Persistence.ConstellationContext.Migrations;
using System.Collections.Generic;

internal class AssignmentResult
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    /// <summary>
    /// The date the assignment is due.
    /// </summary>
    [JsonProperty("due_at")]
    public DateTime? DueDate { get; set; }
    /// <summary>
    /// The date after which submissions are accepted.
    /// </summary>
    [JsonProperty("unlock_at")]
    public DateTime? UnlockDate { get; set; }
    /// <summary>
    /// The date after which no more submission are accepted.
    /// </summary>
    [JsonProperty("lock_at")]
    public DateTime? LockDate { get; set; }
    [JsonProperty("allowed_attempts")]
    public int AllowedAttempts { get; set; }
    [JsonProperty("submission_types")]
    public ICollection<string> SubmissionTypes { get; set; }
    [JsonProperty("published")]
    public bool IsPublished { get; set; }
}