using MediatR;

namespace Constellation.Application.Features.ShortTerm.Covers.Notifications
{
    public class StaffCoverCreatedNotification : INotification
    {
        public int CoverId { get; set; }
    }
}
