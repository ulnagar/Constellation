namespace Constellation.Application.Features.Auth.Queries;

using Constellation.Core.Models.SchoolContacts;
using MediatR;

public sealed class GetSchoolContactByEmailAddressQuery : IRequest<SchoolContact>
{
    public string EmailAddress { get; set; }
}