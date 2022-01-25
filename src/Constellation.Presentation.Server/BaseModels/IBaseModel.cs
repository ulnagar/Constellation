using System.Collections.Generic;

namespace Constellation.Presentation.Server.BaseModels
{
    public interface IBaseModel
    {
        public IDictionary<string, int> Classes { get; set; }

    }
}
