namespace Constellation.Core.Models.Absences;

using Common;

public class ResponseType : StringEnumeration<ResponseType>
{
    public static readonly ResponseType Coordinator = new("Coordinator");
    public static readonly ResponseType Parent = new("Parent");
    public static readonly ResponseType Student = new("Student");
    public static readonly ResponseType System = new("System");

    public ResponseType(string value)
        : base(value, value) { }
}
