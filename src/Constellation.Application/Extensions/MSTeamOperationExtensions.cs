namespace Constellation.Application.Extensions;

using Constellation.Core.Models;
using System;

public static class MsTeamOperationExtensions
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