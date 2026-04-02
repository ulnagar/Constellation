namespace Constellation.Core.Models.Messaging.Enums;

using Common;

public sealed class RecipientGroup : StringEnumeration<RecipientGroup>
{
    public static readonly RecipientGroup AllStaff = new("AllStaff", "All Staff", 1);
    public static readonly RecipientGroup AllExecStaff = new("AllExec", "All Exec Staff", 2);
    public static readonly RecipientGroup AllTeachersOnClassNow = new("AllTeachersOnClassNow", "All Teachers on class now", 3);
    public static readonly RecipientGroup AllTeachersOnClassRestOfDay = new("AllTeachersOnClassRestOfDay", "All Teachers on class for the rest of the day", 5);
    public static readonly RecipientGroup AllACCs = new("AllACCs", "All ACCs", 6);
    public static readonly RecipientGroup AllACCsWithStudentsOnClassNow = new("AllACCsOnClassNow", "All ACCs with Students on class now", 7);
    public static readonly RecipientGroup AllACCsWithStudentsOnClassRestOfDay = new("AllACCsOnClassRestOfDay", "All ACCs with Students on class for the rest of the day", 9);
    public static readonly RecipientGroup AllStudents = new("AllStudents", "All Students", 10);
    public static readonly RecipientGroup AllStudentsOnClassNow = new("AllStudentsOnClassNow", "All Students on class now", 11);
    public static readonly RecipientGroup AllStudentsOnClassRestOfDay = new("AllStudentsOnClassRestOfDay", "All Students on class for the rest of the day", 13);
    public static readonly RecipientGroup AllParents = new("AllParents", "All Residential Parents", 14);
    public static readonly RecipientGroup AllParentsOnClassNow = new("AllParentsOnClassNow", "All Residential Parents with Students on class now", 15);
    public static readonly RecipientGroup AllParentsOnClassRestOfDay = new("AllParentsOnClassRestOfDay", "All Residential Parents with Students on class for the rest of the day", 17);
    
    private RecipientGroup(string value, string name, int order) 
        : base(value, name, order)
    { }

    public static IEnumerable<RecipientGroup> GetOptions => GetEnumerable.OrderBy(entry => entry.Order);
}