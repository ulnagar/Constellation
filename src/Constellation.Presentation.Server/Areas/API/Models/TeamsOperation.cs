namespace Constellation.Presentation.Server.Areas.API.Models
{
    public class TeamsOperation
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string TeamName { get; set; }
        public string UserEmail { get; set; }
        public string AdditionalInformation { get; set; }
        public string Role { get; set; }
        public string Faculty { get; set; }
    }
}
