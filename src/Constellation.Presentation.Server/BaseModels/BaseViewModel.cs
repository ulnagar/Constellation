using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.BaseModels
{
    public class BaseViewModel : IBaseModel
    {
        public BaseViewModel()
        {
            Classes = new Dictionary<string, OfferingId>();
        }

        // Application Settings
        public IDictionary<string, OfferingId> Classes { get; set; }
        public ErrorDisplay Error { get; set; }

    }
}