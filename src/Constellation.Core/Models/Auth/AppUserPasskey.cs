namespace Constellation.Core.Models.Auth;

public sealed class AppUserPasskey
{
    public byte[] CredentialId { get; set; }
    public byte[] PublicKey { get; set; }
    public uint SignatureCounter { get; set; }
    public string CredType { get; set; }
    public Guid AaGuid { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid AppUserId { get; set; }
    public AppUser User { get; set; }
}