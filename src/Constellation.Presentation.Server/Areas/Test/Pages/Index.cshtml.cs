namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Interfaces.Gateways;
using BaseModels;
using Constellation.Application.DTOs;
using Core.Models.Students;
using Core.Models.Students.Enums;
using Core.Models.Students.Repositories;
using MediatR;
using Serilog;
using System.Diagnostics;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IStudentRepository _studentRepository;
    private readonly ISentralGateway _gateway;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        IStudentRepository studentRepository,
        ISentralGateway gateway,
        ILogger logger)
    {
        _mediator = mediator;
        _studentRepository = studentRepository;
        _gateway = gateway;
        _logger = logger;
    }

    public List<string> Indexers { get; set; } = new();
    public List<FamilyDetailsDto> OldMethodResults { get; set; } = new();
    public List<FamilyDetailsDto> NewMethodResults { get; set; } = new();


    public async Task OnGet()
    {
        List<Student> students = await _studentRepository.GetCurrentStudents();

        Stopwatch oldWatch = Stopwatch.StartNew();
        List<FamilyDetailsDto> oldReturn = await OldMethod(students);
        oldWatch.Stop();
        long oldTime = oldWatch.ElapsedMilliseconds;

        Stopwatch newWatch = Stopwatch.StartNew();
        List<FamilyDetailsDto> newReturn = await NewMethod(students);
        newWatch.Stop();
        long newTime = newWatch.ElapsedMilliseconds;

        List<string> oldIndex = oldReturn.Select(entry => entry.FamilyId).ToList();
        List<string> newIndex = newReturn.Select(entry => entry.FamilyId).ToList();

        List<string> jointIndex = new();
        jointIndex.AddRange(oldIndex);
        jointIndex.AddRange(newIndex);
        jointIndex = jointIndex.Distinct().OrderBy(entry => entry).ToList();

        Indexers = jointIndex;

        OldMethodResults = oldReturn;
        NewMethodResults = newReturn;
    }

    private async Task<List<FamilyDetailsDto>> OldMethod(List<Student> students, CancellationToken token = default)
    {
        // Get the CSV file from Sentral
        List<FamilyDetailsDto> families = new();

        Dictionary<string, List<string>> familyGroups = await _gateway.GetFamilyGroupings();
        
        foreach (KeyValuePair<string, List<string>> family in familyGroups)
        {
            Student firstStudent = students.FirstOrDefault(student => student.StudentReferenceNumber.Number == family.Value.First());

            if (firstStudent is null)
                continue;

            SystemLink link = firstStudent.SystemLinks.FirstOrDefault(link => link.System == SystemType.Sentral);

            if (link is null)
                continue;

            FamilyDetailsDto entry = await _gateway.GetParentContactEntry(link.Value);

            entry.StudentReferenceNumbers = family.Value;
            entry.FamilyId = family.Key;

            foreach (FamilyDetailsDto.Contact contact in entry.Contacts)
            {
                string name = contact.FirstName.Contains(' ')
                    ? contact.FirstName.Split(' ')[0]
                    : contact.FirstName;

                name = name.Length > 8 ? name[..8] : name;

                contact.SentralId = $"{entry.FamilyId}-{contact.SentralReference}-{name.ToLowerInvariant()}";
            }

            families.Add(entry);
        }

        return families;
    }

    private async Task<List<FamilyDetailsDto>> NewMethod(List<Student> students, CancellationToken token = default)
    {
        ICollection<FamilyDetailsDto> families = await _gateway.GetFamilyDetailsReportFromApi(_logger, token);

        return families.ToList();
    }
}