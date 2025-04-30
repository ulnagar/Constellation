namespace Constellation.Application.Models.Identity;

using System;

public class AppAccessToken
{
    public Guid AccessToken { get; set; }
    public DateTime Expiry { get; set; }
    public string MapToUser { get; set; }
    public string RedirectTo { get; set; }

    public bool IsCurrent => Expiry >= DateTime.Today;
}