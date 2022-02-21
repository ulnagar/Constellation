using System;

namespace Constellation.Application.Models
{
    public class JobActivation
    {
        public Guid Id { get; set; }
        public string JobName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? InactiveUntil { get; set; }
    }
}
