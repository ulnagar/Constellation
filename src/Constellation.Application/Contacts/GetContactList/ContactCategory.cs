namespace Constellation.Application.Contacts.GetContactList;

using Constellation.Core.Common;

public class ContactCategory : StringEnumeration<ContactCategory>
{
    public static readonly ContactCategory Student = new("Student", "Student");

    public static readonly ContactCategory ResidentialMother = new("Family.Residential.Mother", "Residential Mother");
    public static readonly ContactCategory ResidentialFather = new("Family.Residential.Father", "Residential Father");
    public static readonly ContactCategory ResidentialFamily = new("Family.Residential.Family", "Residential Family");

    public static readonly ContactCategory NonResidentialFamily = new("Family.NonResidential.Family", "Non-Residential Family");
    public static readonly ContactCategory NonResidentialParent = new("Family.NonResidential.Parent", "Non-Residential Parent");

    public static readonly ContactCategory PartnerSchoolSchool = new("PartnerSchool.School", "Partner School");
    public static readonly ContactCategory PartnerSchoolPrincipal = new("PartnerSchool.Principal", "Partner School Principal");
    public static readonly ContactCategory PartnerSchoolACC = new("PartnerSchool.AuroraCollegeCoordinator", "Partner School ACC");
    public static readonly ContactCategory PartnerSchoolSPT = new("PartnerSchool.SciencePracticalTeacher", "Partner School SPT");
    public static readonly ContactCategory PartnerSchoolOtherStaff = new("PartnerSchool.OtherStaff", "Partner School Staff");

    public static readonly ContactCategory AuroraTeacher = new("Aurora.Staff.Teacher", "Class Teacher");
    public static readonly ContactCategory AuroraHeadTeacher = new("Aurora.Staff.HeadTeacher", "Head Teacher");

    private ContactCategory(string value, string name) 
        : base(value, name)
    {
    }
}