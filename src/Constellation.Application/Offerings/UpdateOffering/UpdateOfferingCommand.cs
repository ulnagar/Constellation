﻿namespace Constellation.Application.Offerings.UpdateOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using System;

public sealed record UpdateOfferingCommand(
    OfferingId OfferingId,
    DateOnly StartDate,
    DateOnly EndDate)
    : ICommand;
