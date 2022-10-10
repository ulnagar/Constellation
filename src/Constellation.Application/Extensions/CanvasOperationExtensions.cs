using Constellation.Application.Interfaces.Providers;
using Constellation.Core.Models;

namespace Constellation.Application.Extensions
{
    public static class CanvasOperationExtensions
    {
        public static bool Overdue(this CanvasOperation operation, IDateTimeProvider dateTimeProvider)
        {
            // Has it been marked Deleted or Completed?
            if (operation.ScheduledFor < dateTimeProvider.Now && !(operation.IsDeleted || operation.IsCompleted))
            {
                return true;
            }

            return false;
        }
    }
}
