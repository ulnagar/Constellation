using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    public class ErrorViewModel
    {
        public ErrorViewModel()
        {
            SecondaryMessages = new List<string>();
        }

        public string MainMessage { get; set; }
        public ICollection<string> SecondaryMessages { get; set; }
    }
}