namespace Constellation.Infrastructure.Persistence.ConstellationContext.Audit;

using Constellation.Application.Models.Audit;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

internal class AuditEntry
{
    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
    }

    public EntityEntry Entry { get; }
    public string UserId { get; set; }
    public string TypeName { get; set; }
    public Dictionary<string, object> KeyValues { get; set; } = new();
    public Dictionary<string, object> OldValues { get;set; } = new();
    public Dictionary<string, object> NewValues { get; set; } = new();
    public AuditType AuditType { get; set; }
    public List<string> ChangedColumns { get; } = new();
    
    public Audit ToAudit()
    {
        var audit = new Audit()
        {
            UserId = UserId,
            Type = AuditType.Value,
            TypeName = TypeName,
            DateTime = DateTime.Now,
            PrimaryKey = JsonConvert.SerializeObject(KeyValues),
            OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues),
            NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues),
            AffectedColumns = ChangedColumns.Count == 0 ? null : JsonConvert.SerializeObject(ChangedColumns)
        };
        
        return audit;
    }
}
