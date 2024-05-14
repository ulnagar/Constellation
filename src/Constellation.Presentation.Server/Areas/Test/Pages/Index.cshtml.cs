namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.DTOs;
using Application.Interfaces.Gateways;
using BaseModels;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using MediatR;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ISentralGateway _gateway;
    private readonly IStudentRepository _studentRepository;

    public IndexModel(
        IMediator mediator,
        ISentralGateway gateway,
        IStudentRepository studentRepository)
    {
        _mediator = mediator;
        _gateway = gateway;
        _studentRepository = studentRepository;
    }

    public List<FamilyDetailsDto> Families { get; set; } = new();

    public void OnGet() { }

    public async Task OnGetRunCode()
    {
        Dictionary<string, List<string>> families = await _gateway.GetFamilyGroupings();

        List<Student> students = await _studentRepository.GetCurrentStudentsWithSchool();

        foreach (KeyValuePair<string, List<string>> family in families)
        {
            Student firstStudent = students.FirstOrDefault(student => student.StudentId == family.Value.First());

            if (firstStudent is null)
                continue;

            FamilyDetailsDto entry = await _gateway.GetParentContactEntry(firstStudent.SentralStudentId);

            entry.StudentIds = family.Value;
            entry.FamilyId = family.Key;

            Families.Add(entry);
        }
    }
}