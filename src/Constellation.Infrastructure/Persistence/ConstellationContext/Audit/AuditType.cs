namespace Constellation.Infrastructure.Persistence.ConstellationContext.Audit;

using Constellation.Core.Common;

internal class AuditType : StringEnumeration<AuditType>
{
    public static readonly AuditType None = new("None");
    public static readonly AuditType Create = new("Create");
    public static readonly AuditType Update = new("Update");
    public static readonly AuditType Delete = new("Delete");

    public AuditType(string value) 
        : base(value, value)
    {
    }
}
