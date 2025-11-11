namespace Constellation.Application.Domains.Contacts.Models;

using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed class StudentFlag
{
    public string Name { get; set; }
    public List<StudentId> StudentIds { get; set; } = new();
}