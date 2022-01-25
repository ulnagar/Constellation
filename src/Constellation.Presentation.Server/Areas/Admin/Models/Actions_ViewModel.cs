using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Admin.Models
{
    public class Actions_ViewModel : BaseViewModel
    {
        public List<Actions_OperationDTO> Operations { get; set; }

        public Actions_ViewModel()
        {
            Operations = new List<Actions_OperationDTO>();
        }
    }

    public class Actions_OperationDTO
    {
        public int Id { get; set; }
        public string UserType { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ActionType { get; set; }
        public string RoomId { get; set; }
        public string RoomName { get; set; }
        public DateTime DateScheduled { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
    }
}