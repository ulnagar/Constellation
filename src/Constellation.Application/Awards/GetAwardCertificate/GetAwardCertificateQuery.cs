namespace Constellation.Application.Awards.GetAwardCertificate;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;

public sealed record GetAwardCertificateQuery(
    StudentAwardId AwardId)
    : IQuery<StoredFile>;