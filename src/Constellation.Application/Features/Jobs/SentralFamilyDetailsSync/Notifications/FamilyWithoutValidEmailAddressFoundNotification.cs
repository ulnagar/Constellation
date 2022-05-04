using MediatR;

namespace Constellation.Application.Features.Jobs.SentralFamilyDetailsSync.Notifications
{
    public class FamilyWithoutValidEmailAddressFoundNotification : INotification
    {
        public string FamilyId { get; set; }
    }
}
