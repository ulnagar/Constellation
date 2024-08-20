namespace Constellation.Infrastructure.ExternalServices.Canvas.Models;

using Newtonsoft.Json;

internal class AssignmentSettingsResult
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("rubric")]
    public List<RubricItem> Rubric { get; set; }
    [JsonProperty("rubric_settings")]
    public RubricSettings Settings { get; set; }

    internal sealed class RubricItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("points")]
        public double Points { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("long_description")]
        public string LongDescription { get; set; }
        [JsonProperty("ratings")]
        public List<RubricItem> Ratings { get; set; }
    }

    internal sealed class RubricSettings
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("points_possible")]
        public double PointsPossible { get; set; }
    }
}