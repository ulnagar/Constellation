﻿namespace Constellation.Application.DTOs
{
    using Constellation.Core.Models.Offerings.Identifiers;

    public class SessionDto
    {
        public int Id { get; set; }
        public OfferingId OfferingId { get; set; }
        public string StaffId { get; set; }
        public int PeriodId { get; set; }
        public string RoomId { get; set; }
    }
}
