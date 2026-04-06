namespace Constellation.Application.Domains.Attendance.CheckIns.Queries.GetCheckInResponses;

using Constellation.Core.Enums;
using Core.Models.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CheckInFilter
{
    public List<Guid> Offerings { get; set; } = [];
    public List<Grade> Grades { get; set; } = [];
    public List<SchoolCode> Schools { get; set; } = [];
    public List<Guid> Courses { get; set; } = [];
    public List<string> Sentiments { get; set; } = [];
}