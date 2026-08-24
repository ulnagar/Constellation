namespace Constellation.Application.Domains.Auth.Queries.GetParentUserDetails;

using Core.Enums;
using Core.ValueObjects;

public sealed record ParentUserResponse(
    Guid Id,
    Name Name,
    string Email,
    List<PhoneNumber> PhoneNumbers,
    List<ParentUserResponse.Student> Students,
    List<ParentUserResponse.Passkey> Passkeys)
{
    public sealed record Passkey(
        string Name,
        DateTimeOffset CreatedAt,
        byte[] CredentialId);

    public sealed record Student(
        Name Name,
        Grade Grade,
        string School);
}