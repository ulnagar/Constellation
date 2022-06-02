using MediatR;

namespace Constellation.Application.Features.ShortTerm.Covers.Notifications
{
    public class CasualCoverCreatedNotification : INotification
    {
        public int CoverId { get; set; }
    }
}
