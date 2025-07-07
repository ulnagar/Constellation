namespace Constellation.Application.Domains.Training.Models;

using Core.Models.StaffMembers.Identifiers;
using System.Collections.Generic;

public class StaffCompletionListDto
{
    public StaffId StaffId { get; set; }
    public string Name { get; set; }
    public List<string> Faculties { get; set; }
    public string SchoolName { get; set; }
    public string EmailAddress { get; set; }
    public List<CompletionRecordExtendedDetailsDto> Modules { get; set; } = new();
}
