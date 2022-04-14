using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Admin.Models.AdobeConnect
{
    public class ActionsViewModel : BaseViewModel
    {
        public IEnumerable<AdobeConnectRoomDto> Rooms { get; set; }
        public IEnumerable<AdobeConnectUserDetailDto> Users { get; set; }
    }
}
