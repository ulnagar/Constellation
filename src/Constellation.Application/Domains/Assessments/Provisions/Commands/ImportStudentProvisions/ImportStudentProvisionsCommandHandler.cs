namespace Constellation.Application.Domains.Assessments.Provisions.Commands.ImportStudentProvisions;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Students.ValueObjects;
using Core.Models.Assessments;
using Core.Models.Assessments.Repositories;
using Core.Models.Assessments.ValueObjects;
using Core.Shared;
using OfficeOpenXml;
using Serilog;
using System.Collections.Generic;

internal sealed class ImportStudentProvisionsCommandHandler
: ICommandHandler<ImportStudentProvisionsCommand, List<string>>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    private readonly List<Student> _cachedStudents = [];

    public ImportStudentProvisionsCommandHandler(
        IAssessmentRepository assessmentRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<ImportStudentProvisionsCommand>();
    }

    public async Task<Result<List<string>>> Handle(ImportStudentProvisionsCommand request, CancellationToken cancellationToken)
    {
        List<string> response = [];

        List<Provision> validProvisions = await _assessmentRepository.GetProvisions(cancellationToken);

        List<ImportedStudentProvisions> importData = [];

        using ExcelPackage excel = new(request.ImportFile);
        ExcelWorksheet sheet = excel.Workbook.Worksheets[0];

        int numRows = sheet.Dimension.Rows;

        for (int row = 2; row <= numRows; row++)
        {
            string stringSRN = sheet.Cells[row, 1].GetCellValue<string>();
            List<Provision> provisions = [];

            int numCols = sheet.Dimension.Columns;

            for (int col = 2; col <= numCols; col++)
            {
                string adjustment = sheet.Cells[row, col].GetCellValue<string>();

                if (string.IsNullOrWhiteSpace(adjustment))
                    continue;

                string? stringValue = sheet.Cells[1, col].GetCellValue<string>();
                if (stringValue is null)
                    continue;

                Result<ProvisionCode> provision = ProvisionCode.Create(stringValue);
                Provision? foundProvision = validProvisions.FirstOrDefault(entry => entry.Code == provision.Value);

                if (foundProvision is null)
                {
                    response.Add($"Row {row} Col {col}: Invalid Provision Code '{provision.Value}");

                    continue;
                }

                provisions.Add(foundProvision);
            }

            Result<StudentReferenceNumber> srnResult = StudentReferenceNumber.Create(stringSRN);

            if (srnResult.IsFailure)
            {
                response.Add($"Row {row}: Invalid Student Reference Number '{stringSRN}': {srnResult.Error.Message}");

                continue;
            }

            Student? student = _cachedStudents.FirstOrDefault(student => student.StudentReferenceNumber == srnResult.Value);

            if (student is null)
            {
                student = await _studentRepository.GetBySRN(srnResult.Value, cancellationToken);

                if (student is null)
                {
                    response.Add($"Row {row}: Could not find active student with Student Reference Number '{srnResult.Value}'");

                    continue;
                }

                _cachedStudents.Add(student);
            }

            importData.Add(new()
            {
                Row = row,
                Student = student,
                Provisions = provisions
            });
        }

        foreach (ImportedStudentProvisions entry in importData)
        {
            foreach (Provision provision in entry.Provisions)
            {
                if (await _assessmentRepository.DoesCurrentStudentProvisionExist(entry.Student.Id, provision.Id, DateTime.Today.Year, cancellationToken))
                {
                    response.Add($"Row {entry.Row}: Existing active provision found for {provision.Code}");

                    continue;
                }

                StudentProvision studentProvision = new(provision, entry.Student, DateTime.Today.Year);

                _assessmentRepository.Insert(studentProvision);
            }
            
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return response;
    }

    private sealed class ImportedStudentProvisions
    {
        public required int Row { get; init; }
        public required Student Student { get; init; }
        public List<Provision> Provisions { get; init; } = [];
    }
}
