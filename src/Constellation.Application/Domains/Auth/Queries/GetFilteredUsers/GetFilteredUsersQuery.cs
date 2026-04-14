namespace Constellation.Application.Domains.Auth.Queries.GetFilteredUsers;

using Abstractions.Messaging;
using Core.Models.Auth;
using System.Collections.Generic;

public sealed record GetFilteredUsersQuery(
    UserFilter Filter = UserFilter.All)
    : IQuery<List<AppUser>>;