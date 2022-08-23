namespace Constellation.Application.DTOs.EmailRequests;

public class MagicLinkEmail : EmailBaseClass
{
    public string Name { get; set; }
    public string Link { get; set; }
}
