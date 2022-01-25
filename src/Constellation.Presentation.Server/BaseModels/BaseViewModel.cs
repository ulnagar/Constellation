using System.Collections.Generic;

namespace Constellation.Presentation.Server.BaseModels
{
    public class BaseViewModel : IBaseModel
    {
        public BaseViewModel()
        {
            Classes = new Dictionary<string, int>();
        }

        // Application Settings
        public IDictionary<string, int> Classes { get; set; }
    }
}