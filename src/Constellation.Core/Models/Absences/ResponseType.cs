namespace Constellation.Core.Models.Absences;

using Constellation.Core.Common;

public class ResponseType : StringEnumeration<ResponseType>
{
    public static ResponseType Coordinator = new("Coordinator");
    public static ResponseType Parent = new("Parent");
    public static ResponseType Student = new("Student");
    public static ResponseType System = new("System");

    public ResponseType(string value)
        : base(value, value) { }
}
