using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Constellation.Application.Interfaces.Jobs.AbsenceClassworkNotificationJob;

public static class Models
{
    public class AbsenceDto
    {
        public Guid Id { get; set; }
        public string OfferingId { get; set; }
        public DateTime Date { get; set; }
        public List<(string Id, string DisplayName)> Teachers { get; set; } = new();
        public List<(string Id, string DisplayName)> HeadTeachers { get; set; } = new();
        public (string Id, string DisplayName) Student { get; set; }
    }
}
