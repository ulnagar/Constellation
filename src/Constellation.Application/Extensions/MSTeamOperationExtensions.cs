using Constellation.Core.Models;
using System;

namespace Constellation.Application.Extensions
{
    public static class MSTeamOperationExtensions
    {
        public static bool IsOutstanding(this MSTeamOperation operation)
        {
            // Has it been marked Deleted or Completed?
            if (operation.DateScheduled <= DateTime.Today && !(operation.IsDeleted || operation.IsCompleted))
            {
                return true;
            }

            return false;
        }
    }
}
