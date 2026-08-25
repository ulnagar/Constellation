namespace Constellation.Application.Domains.Auth.Queries.GetParentUserDetails;

using Core.Models.SchoolContacts.Enums;
using Core.ValueObjects;

public sealed record ContactUserResponse(
    Guid Id,
    Name Name,
    string Email,
    List<ContactUserResponse.Role> Roles,
    List<ContactUserResponse.Passkey> Passkeys)
{
    public sealed record Passkey(
        string Name,
        DateTimeOffset CreatedAt,
        byte[] CredentialId);

    public sealed record Role(
        Position Position,
        string School);
}