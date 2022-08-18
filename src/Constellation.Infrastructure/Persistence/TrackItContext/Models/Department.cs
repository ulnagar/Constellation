using System;

namespace Constellation.Infrastructure.Persistence.TrackItContext.Models;

public partial class Department
{
    public int Sequence { get; set; }
    public DateTime Lastmodified { get; set; }
    public string Lastuser { get; set; }
    public int? Group { get; set; }
    public string Note { get; set; }
    public string Dept { get; set; }
    public string Name { get; set; }
    public int? Location { get; set; }
    public string Phone { get; set; }
    public string Fax { get; set; }
    public int? SeqDeptmanager { get; set; }
    public int? Assistmanager { get; set; }
    public int? SeqPriority { get; set; }
    public short Inactive { get; set; }
}
