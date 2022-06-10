using MediatR;

namespace Constellation.Application.Features.ShortTerm.Covers.Notifications
{
    public class StaffCoverCancelledNotification : INotification
    {
        public int CoverId { get; set; }
    }
}
