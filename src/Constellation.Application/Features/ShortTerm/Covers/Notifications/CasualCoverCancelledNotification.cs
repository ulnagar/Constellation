using MediatR;
using System;

namespace Constellation.Application.Features.ShortTerm.Covers.Notifications
{
    public class CasualCoverCancelledNotification : INotification
    {
        public Guid CoverId { get; set; }
    }
}
