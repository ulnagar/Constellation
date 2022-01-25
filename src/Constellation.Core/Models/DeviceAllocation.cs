using System;

namespace Constellation.Core.Models
{
    public class DeviceAllocation
    {
        public DeviceAllocation()
        {
            IsDeleted = false;
            DateAllocated = DateTime.Now;
        }

        public DeviceAllocation(string studentId, string serialNumber)
        {
            StudentId = studentId;
            SerialNumber = serialNumber;
        }

        public int Id { get; set; }
        public Student Student { get; set; }
        public string StudentId { get; set; }
        public Device Device { get; set; }
        public string SerialNumber { get; set; }
        public DateTime DateAllocated { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateDeleted { get; set; }

        public void Delete()
        {
            IsDeleted = true;
            DateDeleted = DateTime.Now;
        }
    }
}