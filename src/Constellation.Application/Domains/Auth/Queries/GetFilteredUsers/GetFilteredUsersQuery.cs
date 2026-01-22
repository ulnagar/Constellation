namespace Constellation.Application.Domains.Auth.Queries.GetFilteredUsers;

using Abstractions.Messaging;
using Models.Identity;
using System.Collections.Generic;

public sealed record GetFilteredUsersQuery(
    UserFilter Filter = UserFilter.All)
    : IQuery<List<AppUser>>;