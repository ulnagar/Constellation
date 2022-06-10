using MediatR;

namespace Constellation.Application.Features.ShortTerm.Covers.Notifications
{
    public class CasualCoverCancelledNotification : INotification
    {
        public int CoverId { get; set; }
    }
}
