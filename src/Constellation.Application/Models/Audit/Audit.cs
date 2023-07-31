namespace Constellation.Application.Models.Audit;

using System;

public sealed class Audit
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Type { get; set; }
    public string TypeName { get; set; }
    public DateTime DateTime { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public string AffectedColumns { get; set; }
    public string PrimaryKey { get; set; }
}
