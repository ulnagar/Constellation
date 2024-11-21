namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Interfaces.Gateways;
using BaseModels;
using Constellation.Core.Models.Students;
using Core.Abstractions.Repositories;
using Core.Models.Families;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.IO;
using System.Net.Mail;
using System.Text;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IEmailGateway _emailGateway;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IEmailGateway emailGateway,
        ILogger logger)
    {
        _mediator = mediator;
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _emailGateway = emailGateway;
        _logger = logger;
    }

    public async Task OnGet() { }
    
    public async Task<IActionResult> OnGetDownload()
    {
        Dictionary<string, string[]> studentMap = new();

        string path = @"c:\users\bhillsley1\Desktop\PAT Reports S2 2024\Combined";

        string[] files = Directory.GetFiles(path);

        foreach (var file in files)
        {
            string name = Path.GetFileName(file);

            if (name[0] != '4')
                continue;

            string srn = name.Split('-')[0];

            if (studentMap.ContainsKey(srn))
            {
                var entry = studentMap[srn].Append(file).ToArray();
                studentMap[srn] = entry;
            }
            else
            {
                studentMap[srn] = new[] { file };
            }
        }
        
        foreach (var group in studentMap)
        {
            StudentReferenceNumber srn = StudentReferenceNumber.FromValue(group.Key);

            Student student = await _studentRepository.GetBySRN(srn);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(StudentReferenceNumber), group.Key)
                    .Warning("Student could not be found");
                
                continue;
            }

            if (student.IsDeleted)
            {
                _logger
                    .ForContext(nameof(StudentReferenceNumber), group.Key)
                    .ForContext(nameof(Student), student, true)
                    .Warning("Student is no longer enrolled");

                continue;
            }

            List<Attachment> attachments = new();

            foreach (var file in group.Value)
            {
                attachments.Add(new Attachment(file));
            }

            List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.Id);

            foreach (Family family in families)
            {
                List<EmailRecipient> sentMessages = new();

                string familyBody = GetEmailBody(family.FamilyTitle);

                Result<EmailRecipient> familyRecipient = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

                if (familyRecipient.IsFailure)
                    continue;

                _logger
                    .ForContext(nameof(EmailRecipient), familyRecipient, true)
                    .Information("Sending email to family");

                await _emailGateway.Send(
                    [familyRecipient.Value], 
                    "auroracoll-h.school@det.nsw.edu.au",
                    "Additional Student Report - Semester 2 2024 - Progressive Achievement Test", 
                    familyBody,
                    attachments);

                sentMessages.Add(familyRecipient.Value);

                foreach (Parent parent in family.Parents)
                {
                    string parentBody = GetEmailBody($"{parent.Title} {parent.FirstName} {parent.LastName}");

                    Result<EmailRecipient> parentRecipient = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

                    if (parentRecipient.IsFailure)
                        continue;

                    if (sentMessages.Any(entry => entry.Email == parentRecipient.Value.Email))
                        continue;

                    _logger
                        .ForContext(nameof(EmailRecipient), parentRecipient, true)
                        .ForContext(nameof(Parent), parent, true)
                        .Information("Sending email to parent");

                    await _emailGateway.Send(
                        [parentRecipient.Value],
                        "auroracoll-h.school@det.nsw.edu.au",
                        "Additional Student Report - Semester 2 2024 - Progressive Achievement Test",
                        parentBody,
                        attachments);

                    sentMessages.Add(parentRecipient.Value);
                }
            }
        }

        return Page();
    }

    private string GetEmailBody(string parentName)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"<p>Dear {parentName}</p>");
        builder.AppendLine("<p>In supporting growth and development of the fundamental skills in Literacy and Numeracy of our students, Aurora College will be tracking progress using Progressive Achievement Tests (PAT) testing. PAT are a series of tests designed to provide objective, norm-referenced information to teachers about their students’ skills and understandings in a range of key areas. Online PAT tests are conducted twice per year to devise strategies to ensure that Aurora College is driving progress in Literacy and Numeracy. The data gathered from these tests are used to inform teaching practices and promote differentiation. The information will also allow staff to design individualised approaches to address areas for improvement. The student’s results will be provided to families during Semester 1 and Semester 2 each year.</p>");
        builder.AppendLine("<p>Using this data, classroom teachers are integrating targeted and individualised intervention strategies to ensure that students are enhancing their skills in Literacy and Numeracy. This may include in-class activities, specialised group lessons, and homework tasks.</p>");
        builder.AppendLine("<p>The PAT testing for this report was held during Term 3 2024.</p>");
        builder.AppendLine("<ul>");
        builder.AppendLine("<li>Raw score. A raw score is the number of score points achieved on a test. In the case of PAT, it is the number of questions answered correctly on the test.</li>");
        builder.AppendLine("<li>Scale score. PAT student achievement is measured in PAT scale scores. PAT tests within a learning area are all equated onto a common scale. This scale enables student achievement and question difficulties to be located on the same scale.</li>");
        builder.AppendLine("<li>Percentile. The percentile rank is NOT the percentage correct achieved on a test. In general terms, the percentile rank of a score is the percentage of students who achieve less than that score. For example, a student with a percentile rank of 75 on a Year 3 test has a score that is higher than 75% of Australian Year 3 students.</li>");
        builder.AppendLine("<li>Stanine. A stanine provides a coarser ranking than the percentile rank. Stanines divide the total student distribution of abilities into nine categories, with stanine 1 the lowest, stanine 5 the midpoint and stanine 9 the highest.</li>");
        builder.AppendLine("<li>Confidence band. A confidence band is the range of values surrounding an estimate or statistic within which we are fairly confident the true value lies. It describes the uncertainty associated with knowing what the actual mean is, especially when taking into account many variables, such as students not performing well that day due to illness or variance in testing conditions such as location, noise level, etc.</li>");
        builder.AppendLine("</ul>");
        builder.AppendLine("<p>PAT norms are established by taking a representative sample of Australian students at each year level to form the norm reference sample. The reference samples consist of students, both boys and girls, from all States and Territories and all of the educational sectors: Government, Catholic and Independent. Results from PAT tests administered to these reference samples of Australian students are used to ascertain the average scores and standard deviations of each year level, and (assuming a normal distribution) to calculate the set of percentile ranks associated with achieved scale scores.</p>");
        builder.AppendLine("<p>If you have any queries regarding these reports, please do not hesitate to contact Aurora College via phone on 1300 287 629 or via email at auroracoll-h.school@det.nsw.edu.au");
        builder.AppendLine("<br/><br/>");
        builder.AppendLine("<p>Julie Ruming</p>");
        builder.AppendLine("<p>Head Teacher Wellbeing</p>");

        return builder.ToString();
    }
}