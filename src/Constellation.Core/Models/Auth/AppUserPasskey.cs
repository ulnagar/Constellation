namespace Constellation.Core.Models.Auth;

public sealed class AppUserPasskey
{
    public required byte[] CredentialId { get; init; }
    public required byte[] PublicKey { get; init; }
    public uint SignatureCounter { get; set; }
    public required string CredType { get; init; }
    public required Guid AaGuid { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required Guid AppUserId { get; init; }
    public AppUser? User { get; init; }
    public required string Name { get; init; }
}