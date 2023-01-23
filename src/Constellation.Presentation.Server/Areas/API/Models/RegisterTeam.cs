using System;

namespace Constellation.Presentation.Server.Areas.API.Models
{
    public class CreateTeamResource
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string GeneralChannelId { get; set; }
    }
}
