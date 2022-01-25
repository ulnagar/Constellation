using Constellation.Core.Models;
using System;

namespace Constellation.Application.Extensions
{
    public static class AdobeConnectOperationExtensions
    {
        public static bool IsOutstanding(this AdobeConnectOperation operation)
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
