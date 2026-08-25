namespace Constellation.Application.Domains.MeritAwards.Awards.Models;

using Core.Enums;
using Core.ValueObjects;

public sealed record StudentAwardTally(
    Name Student,
    Grade Grade,
    int Astras,
    int Stellars,
    int Galaxies,
    int Universals,
    int Other);