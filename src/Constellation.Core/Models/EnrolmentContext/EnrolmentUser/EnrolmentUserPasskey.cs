namespace Constellation.Core.Models.EnrolmentContext.EnrolmentUser;

public sealed class EnrolmentUserPasskey
{
    public required byte[] CredentialId { get; init; }
    public required byte[] PublicKey { get; init; }
    public uint SignatureCounter { get; set; }
    public required string CredType { get; init; }
    public required Guid AaGuid { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required Guid UserId { get; init; }
    public EnrolmentUser? User { get; init; }
    public required string Name { get; init; }
}