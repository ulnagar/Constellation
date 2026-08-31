namespace Constellation.Application.Domains.Auth.Models;

using System;
using System.Collections.Generic;
using System.Text;

public sealed record AuthenticatorMetadataEntry(string Name, string? IconUrl);