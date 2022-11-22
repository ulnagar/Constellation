namespace Constellation.Application.Features.Faculties.Models;

using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using System;

public class FacultyEditContextDto : IMapFrom<Faculty>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Colour { get; set; } 
}
