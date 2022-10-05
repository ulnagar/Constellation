using Constellation.Infrastructure.Templates.Views.Shared;

namespace Constellation.Infrastructure.Templates.Views.Emails.Auth
{
    public class MagicLinkLoginEmailViewModel : EmailLayoutBaseViewModel
    {
        public string ToName { get; set; }
        public string Link { get; set; }
    }
}
