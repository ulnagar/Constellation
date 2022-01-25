using System.Collections.Generic;
namespace Constellation.Application.DTOs
{
    public class ServiceOperationResult<TEntity> where TEntity : class
    {
        public bool Success { get; set; }
        public TEntity Entity { get; set; }
        public ICollection<string> Errors { get; set; }

        public ServiceOperationResult()
        {
            Errors = new List<string>();
        }
    }
}
