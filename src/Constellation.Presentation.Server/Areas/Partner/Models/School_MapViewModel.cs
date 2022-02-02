using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class School_MapViewModel : BaseViewModel
    {
        public School_MapViewModel()
        {
            Layers = new List<MapLayer>();
        }

        public IList<MapLayer> Layers { get; set; }
    }
}
