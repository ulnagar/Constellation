namespace Constellation.Core.Models;

public class Casual
{
    public Casual()
    {
        IsDeleted = false;
        DateEntered = DateTime.Now;

        ClassCovers = new List<CasualClassCover>();
        AdobeConnectOperations = new List<CasualAdobeConnectOperation>();
        MSTeamOperations = new List<CasualMSTeamOperation>();
    }

    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PortalUsername { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public DateTime? DateEntered { get; set; }
    public string AdobeConnectPrincipalId { get; set; } = string.Empty;
    public string SchoolCode { get; set; } = string.Empty;
    public virtual School? School { get; set; }
    public string EmailAddress => PortalUsername + "@det.nsw.edu.au";
    public string DisplayName => FirstName + " " + LastName;
    public ICollection<CasualClassCover> ClassCovers { get; set; }
    public ICollection<CasualAdobeConnectOperation> AdobeConnectOperations { get; set; }
    public ICollection<CasualMSTeamOperation> MSTeamOperations { get; set; }
}