namespace Constellation.Infrastructure.Services;

using Application.Absences.ExportUnexplainedPartialAbsencesReport;
using Application.Absences.GetAbsencesWithFilterForReport;
using Application.Assets.ImportAssetsFromFile;
using Application.Attendance.GenerateAttendanceReportForPeriod;
using Application.Attendance.GetAttendanceDataFromSentral;
using Application.Awards.ExportAwardNominations;
using Application.Compliance.GetWellbeingReportFromSentral;
using Application.Contacts.Models;
using Application.DTOs;
using Application.DTOs.Canvas;
using Application.DTOs.CSV;
using Application.Extensions;
using Application.ExternalDataConsistency;
using Application.GroupTutorials.GenerateTutorialAttendanceReport;
using Application.Helpers;
using Application.SchoolContacts.GetContactsBySchool;
using Application.SciencePracs.GenerateOverdueReport;
using Application.Training.Models;
using Application.WorkFlows.ExportOpenCaseReport;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Students.ImportStudentsFromFile;
using Constellation.Application.Training.GenerateOverallReport;
using Constellation.Core.Models.Assets;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Extensions;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.Models.Training;
using Core.Shared;
using ExcelDataReader;
using Jobs;
using OfficeOpenXml;
using OfficeOpenXml.ConditionalFormatting.Contracts;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table.PivotTable;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

public class ExcelService : IExcelService
{
    private readonly IDateTimeProvider _dateTime;
    private static readonly Regex _csvParser = new(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
    private static readonly List<string[]> _ptoHeadings = new()
    {
        new[]
        {
            "SCD", "SFN", "SSN", "PCD1", "PTI1", "PFN1", "PSN1", "PEM1", "PCD2", "PTI2", "PFN2", "PSN2", "PEM2",
            "CCD", "CGN", "CLS", "TCD", "TTI", "TFN", "TSN", "TEM"
        }
    };
    private static readonly List<string[]> _awardsHeadings = new()
    {
        new[] { "Student Id", "Student Name", "Grade", "Stellars", "Galaxies", "Universal Achievers" }
    };

    public ExcelService(
        IDateTimeProvider dateTime)
    {
        _dateTime = dateTime;
    }

    public Task<List<ImportStudentDto>> ImportStudentsFromFile(
        MemoryStream stream,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new(stream);
        ExcelWorksheet worksheet = excel.Workbook.Worksheets[0];

        int numRows = worksheet.Dimension.Rows;
        int numCols = worksheet.Dimension.Columns;

        int srnRow = 1;
        int firstNameRow = 2;
        int lastNameRow = 3;
        int emailRow = 4;
        int genderRow = 5;
        int gradeRow = 6;
        int schoolRow = 7;

        for (int col = 1; col <= numCols; col++)
        {
            string value = worksheet.Cells[1, col].GetCellValue<string>();

            if (string.IsNullOrEmpty(value))
                continue;

            value = value.Trim();

            switch (value)
            {
                case "Student No.":
                    srnRow = col;
                    break;
                case "Student_First":
                    firstNameRow = col;
                    break;
                case "Student_Last":
                    lastNameRow = col;
                    break;
                case "Gender":
                    genderRow = col;
                    break;
                case "Cohort":
                    gradeRow = col;
                    break;
                case "School":
                    schoolRow = col;
                    break;
                case "Student_Email":
                    emailRow = col;
                    break;
            }
        }

        List<ImportStudentDto> students = new();

        for (int row = 2; row <= numRows; row++)
        {
            ImportStudentDto entry = new(
                row,
                worksheet.Cells[row, srnRow].GetCellValue<string>()?.Trim(),
                worksheet.Cells[row, firstNameRow].GetCellValue<string>()?.Trim(),
                worksheet.Cells[row, lastNameRow].GetCellValue<string>()?.Trim(),
                worksheet.Cells[row, emailRow].GetCellValue<string>()?.Trim(),
                worksheet.Cells[row, genderRow].GetCellValue<string>()?.Trim(),
                worksheet.Cells[row, gradeRow].GetCellValue<string>()?.Trim(),
                worksheet.Cells[row, schoolRow].GetCellValue<string>()?.Trim());

            students.Add(entry);
        }

        excel.Dispose();
        return Task.FromResult(students);
    }

    public Task<List<ImportAssetDto>> ImportAssetsFromFile(
        MemoryStream stream,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new(stream);
        ExcelWorksheet worksheet = excel.Workbook.Worksheets[0];

        int numRows = worksheet.Dimension.Rows;

        List<ImportAssetDto> assets = new();

        for (int row = 2; row <= numRows; row++)
        {
            ImportAssetDto entry = new(
                row,
                worksheet.Cells[row, 1].GetCellValue<string>(),
                worksheet.Cells[row, 2].GetCellValue<string>(),
                worksheet.Cells[row, 3].GetCellValue<string>(),
                worksheet.Cells[row, 4].GetCellValue<string>(),
                worksheet.Cells[row, 5].GetCellValue<string>(),
                worksheet.Cells[row, 6].GetCellValue<string>(),
                worksheet.Cells[row, 7].GetCellValue<string>(),
                worksheet.Cells[row, 8].GetCellValue<string>(),
                DateOnly.FromDateTime(worksheet.Cells[row, 9].GetCellValue<DateTime>()),
                worksheet.Cells[row, 10].GetCellValue<decimal>(),
                DateOnly.FromDateTime(worksheet.Cells[row, 11].GetCellValue<DateTime>()),
                worksheet.Cells[row, 12].GetCellValue<string>(),
                worksheet.Cells[row, 13].GetCellValue<string>(),
                worksheet.Cells[row, 14].GetCellValue<string>(),
                worksheet.Cells[row, 15].GetCellValue<string>(),
                worksheet.Cells[row, 17].GetCellValue<string>(),
                DateOnly.FromDateTime(worksheet.Cells[row, 16].GetCellValue<DateTime>()));
            
            assets.Add(entry);
        }

        excel.Dispose();
        return Task.FromResult(assets);
    }

    public async Task<MemoryStream> CreateAssetExportFile(
        List<Asset> assets,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();

        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Sheet 1");

        worksheet.Cells[1, 1].Value = "Asset Number";
        worksheet.Cells[1, 2].Value = "Serial Number";
        worksheet.Cells[1, 3].Value = "SAP Equipment Number";
        worksheet.Cells[1, 4].Value = "Manufacturer";
        worksheet.Cells[1, 5].Value = "Model Number";
        worksheet.Cells[1, 6].Value = "Model Description";
        worksheet.Cells[1, 7].Value = "Status";
        worksheet.Cells[1, 8].Value = "Device Category";
        worksheet.Cells[1, 9].Value = "Purchase Date";
        worksheet.Cells[1, 10].Value = "Purchase Cost";
        worksheet.Cells[1, 11].Value = "Warranty End Date";
        worksheet.Cells[1, 12].Value = "Location Category";
        worksheet.Cells[1, 13].Value = "Location Site";
        worksheet.Cells[1, 14].Value = "Location Room";
        worksheet.Cells[1, 15].Value = "Responsible Officer";
        worksheet.Cells[1, 16].Value = "Last Seen";
        worksheet.Cells[1, 17].Value = "Last Seen By";
        worksheet.Cells[1, 18].Value = "Notes";

        int row = 2;

        foreach (Asset asset in assets)
        {
            worksheet.Cells[row, 1].Value = asset.AssetNumber;
            worksheet.Cells[row, 2].Value = asset.SerialNumber;
            worksheet.Cells[row, 3].Value = asset.SapEquipmentNumber;
            worksheet.Cells[row, 4].Value = asset.Manufacturer;
            worksheet.Cells[row, 5].Value = asset.ModelNumber;
            worksheet.Cells[row, 6].Value = asset.ModelDescription;
            worksheet.Cells[row, 7].Value = asset.Status.Name;
            worksheet.Cells[row, 8].Value = asset.Category.Name;

            if (asset.PurchaseDate == DateOnly.MinValue)
            {
                worksheet.Cells[row, 9].Value = string.Empty;
            }
            else
            {
                worksheet.Cells[row, 9].Value = asset.PurchaseDate.ToDateTime(TimeOnly.MinValue);
                worksheet.Cells[row, 9].Style.Numberformat.Format = "dd/MM/yyyy";
            }

            if (asset.PurchaseCost == 0m)
            {
                worksheet.Cells[row, 10].Value = string.Empty;
            }
            else 
            {
                worksheet.Cells[row, 10].Value = asset.PurchaseCost;
                worksheet.Cells[row, 10].Style.Numberformat.Format = "_-$* #,##0.00_-;-$* #,##0.00_-;_-$* \"-\"_-;_-@_-";
            }

            if (asset.WarrantyEndDate == DateOnly.MinValue)
            {
                worksheet.Cells[row, 11].Value = string.Empty;
            }
            else
            {
                worksheet.Cells[row, 11].Value = asset.WarrantyEndDate.ToDateTime(TimeOnly.MinValue);
                worksheet.Cells[row, 11].Style.Numberformat.Format = "dd/MM/yyyy";
            }

            worksheet.Cells[row, 12].Value = asset.CurrentLocation?.Category.Name;
            worksheet.Cells[row, 13].Value = asset.CurrentLocation?.Site;
            worksheet.Cells[row, 14].Value = asset.CurrentLocation?.Room;
            worksheet.Cells[row, 15].Value = asset.CurrentAllocation?.ResponsibleOfficer;

            if (asset.LastSighting is null || asset.LastSighting.SightedAt == DateTime.MinValue)
            {
                worksheet.Cells[row, 16].Value = string.Empty;
            }
            else
            {
                worksheet.Cells[row, 16].Value = asset.LastSighting.SightedAt;
                worksheet.Cells[row, 16].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
            }

            worksheet.Cells[row, 17].Value = asset.LastSighting?.SightedBy;

            if (asset.Notes.Count > 0)
            {
                IOrderedEnumerable<Note> orderedNotes = asset.Notes.OrderByDescending(note => note.CreatedAt);
                IEnumerable<string> noteText = orderedNotes.Select(note => $"{note.CreatedAt} - {note.CreatedBy} - {note.Message}");

                worksheet.Cells[row, 18].RichText.Add(string.Join("\r\n", noteText));
                worksheet.Cells[row, 18].Style.WrapText = true;
                worksheet.Cells[row, 18].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            }

            row++;
        }

        worksheet.Columns[18].Width = 80;
        worksheet.View.FreezePanes(2, 1);
        worksheet.Cells[1, 1, row, 18].AutoFilter = true;
        worksheet.Cells[1, 1, row, 17].AutoFitColumns();

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreatePTOFile(
        List<InterviewExportDto> exportLines, 
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();
        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("PTO");
        
        worksheet.Cells[1, 1, 1, _ptoHeadings[0].Length].LoadFromArrays(_ptoHeadings);

        List<string[]> rowData = new();

        exportLines ??= new();

        foreach (InterviewExportDto line in exportLines)
        {
            List<string> row = new()
            {
                line.StudentId, 
                line.StudentFirstName, 
                line.StudentLastName
            };

            foreach (InterviewExportDto.Parent parent in line.Parents)
            {
                row.Add(parent.ParentCode);
                row.Add(parent.ParentTitle);
                row.Add(parent.ParentFirstName);
                row.Add(parent.ParentLastName);
                row.Add(parent.ParentEmailAddress);
            }

            if (line.Parents.Count == 1)
            {
                row.Add("");
                row.Add("");
                row.Add("");
                row.Add("");
                row.Add("");
            }

            row.Add(line.ClassCode);
            row.Add(line.ClassGrade);
            row.Add(line.ClassName);

            row.Add(line.TeacherCode);
            row.Add(line.TeacherTitle);
            row.Add(line.TeacherFirstName);
            row.Add(line.TeacherLastName);
            row.Add(line.TeacherEmailAddress);

            rowData.Add(row.ToArray());
        }

        worksheet.Cells[2, 1].LoadFromArrays(rowData);

        MemoryStream stream = new();
        await excel.SaveAsAsync(stream, cancellationToken);
        stream.Position = 0;

        excel.Dispose();
        return stream;
    }

    public async Task<MemoryStream> CreateAbsencesReportFile(
        List<FilteredAbsenceResponse> exportAbsences, 
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Sheet 1");
        
        workSheet.Cells[2, 1].LoadFromCollection(exportAbsences, true);
        workSheet.Cells[2, 6, workSheet.Dimension.Rows, 6].Style.Numberformat.Format = "dd/MM/yyyy";
        workSheet.Cells[2, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateUnexplainedPartialAbsencesReportFile(
        List<UnexplainedPartialAbsenceResponse> absences,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();
        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Sheet 1");

        worksheet.Cells[1, 1].LoadFromCollection(absences, opt =>
        {
            opt.PrintHeaders = true;
            opt.TableStyle = OfficeOpenXml.Table.TableStyles.Light9;
            opt.HeaderParsingType = OfficeOpenXml.LoadFunctions.Params.HeaderParsingTypes.CamelCaseToSpace;
        });

        for (int i = 1; i <= worksheet.Dimension.Rows; i++)
        {
            if (i == 1)
                continue;

            string absenceDateString = worksheet.Cells[i, 7].Value?.ToString();

            if (absenceDateString is not null)
                worksheet.Cells[i, 7].Value = DateTime.Parse(absenceDateString, null);

            string responseDateString = worksheet.Cells[i, 12].Value?.ToString();

            if (responseDateString is not null)
                worksheet.Cells[i, 12].Value = DateTime.Parse(responseDateString, null);
        }
        
        worksheet.Columns[7].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
        worksheet.Columns[9].Style.Numberformat.Format = "0";
        worksheet.Columns[12].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;

        worksheet.View.FreezePanes(2, 1);
        worksheet.Cells[1, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns].AutoFitColumns();

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateTrainingModuleReportFile(ModuleDetailsDto data)
    {
        Type completion = typeof(CompletionRecordDto);

        ExcelPackage excel = new();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Sheet 1");

        if (data is null)
        {
            MemoryStream failedStream = new();
            await excel.SaveAsAsync(failedStream);
            failedStream.Position = 0;

            excel.Dispose();
            return failedStream;
        }
        
        ExcelRichText nameDetail = workSheet.Cells[1, 1].RichText.Add(data.Name);
        nameDetail.Bold = true;
        nameDetail.Size = 16;
        
        workSheet.Cells[2, 1].Value = $"Exported at {DateTime.Now:F}";
        workSheet.Cells[3, 1].Value = $"Link to module: {data.Url}";

        workSheet.Cells[7, 1].LoadFromCollection(data.Completions, opt =>
        {
            opt.PrintHeaders = true;
            opt.TableStyle = OfficeOpenXml.Table.TableStyles.Light1;
            opt.HeaderParsingType = OfficeOpenXml.LoadFunctions.Params.HeaderParsingTypes.CamelCaseToSpace;
            opt.Members = new MemberInfo[]
            {
                completion.GetProperty(nameof(CompletionRecordDto.StaffId)),
                completion.GetProperty(nameof(CompletionRecordDto.StaffFirstName)),
                completion.GetProperty(nameof(CompletionRecordDto.StaffLastName)),
                completion.GetProperty(nameof(CompletionRecordDto.StaffFaculty)),
                completion.GetProperty(nameof(CompletionRecordDto.Mandatory)),
                completion.GetProperty(nameof(CompletionRecordDto.ExpiryCountdown)),
                completion.GetProperty(nameof(CompletionRecordDto.CompletedDate))
            };
        });

        workSheet.Cells[7, 7, workSheet.Dimension.Rows, 7].Style.Numberformat.Format = "dd/MM/yyyy";

        // Highlight overdue entries
        ExcelAddress dataRange = new(8, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns);

        IExcelConditionalFormattingExpression formatNotRequired = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatNotRequired.Formula = "=$E8 = FALSE";
        formatNotRequired.Style.Font.Color.Color = Color.DarkOliveGreen;
        formatNotRequired.Style.Font.Italic = true;
        formatNotRequired.StopIfTrue = true;

        IExcelConditionalFormattingExpression formatNeverCompleted = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatNeverCompleted.Formula = "=$F8 = -9999";
        formatNeverCompleted.Style.Fill.BackgroundColor.Color = Color.Gray;
        formatNeverCompleted.Style.Font.Color.Color = Color.White;
        formatNeverCompleted.StopIfTrue = true;

        IExcelConditionalFormattingExpression formatOverdue = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatOverdue.Formula = "=$F8 < 1";
        formatOverdue.Style.Fill.BackgroundColor.Color = Color.Red;
        formatOverdue.Style.Font.Color.Color = Color.White;
        formatOverdue.StopIfTrue = true;

        IExcelConditionalFormattingExpression formatSoonExpire = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatSoonExpire.Formula = "=$F8 < 14";
        formatSoonExpire.Style.Fill.BackgroundColor.Color = Color.Yellow;
        formatSoonExpire.StopIfTrue = true;

        // Add format legend
        workSheet.Cells[5, 2].Value = "Colour Legend";
        workSheet.Cells[5, 2].Style.Font.Bold = true;
        workSheet.Cells[5, 3].Value = "Expired";
        workSheet.Cells[5, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
        workSheet.Cells[5, 3].Style.Fill.BackgroundColor.SetColor(Color.Red);
        workSheet.Cells[5, 3].Style.Font.Color.SetColor(Color.White);
        workSheet.Cells[5, 4].Value = "Expiring Soon";
        workSheet.Cells[5, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
        workSheet.Cells[5, 4].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
        workSheet.Cells[5, 5].Value = "Never Completed";
        workSheet.Cells[5, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
        workSheet.Cells[5, 5].Style.Fill.BackgroundColor.SetColor(Color.Gray);
        workSheet.Cells[5, 5].Style.Font.Color.SetColor(Color.White);
        workSheet.Cells[5, 6].Value = "Not Required";
        workSheet.Cells[5, 6].Style.Font.Color.SetColor(Color.DarkOliveGreen);
        workSheet.Cells[5, 6].Style.Font.Italic = true;
        workSheet.Cells[5, 2, 5, 6].Style.Border.BorderAround(ExcelBorderStyle.Thick);
        workSheet.Cells[5, 3, 5, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // Freeze top rows
        workSheet.View.FreezePanes(8, 1);
        workSheet.Cells[5, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateTrainingModuleStaffReportFile(StaffCompletionListDto data)
    {
        Type completion = typeof(CompletionRecordExtendedDetailsDto);

        ExcelPackage excel = new();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Sheet 1");

        data ??= new();

        ExcelRichText nameDetail = workSheet.Cells[1, 1].RichText.Add(data.Name);
        nameDetail.Bold = true;
        nameDetail.Size = 16;

        string facultyText = "Faculties: ";
        for (int i = 0; i < data.Faculties.Count; i++)
        {
            if (i != 0)
            {
                facultyText += ", ";
            }

            facultyText += data.Faculties[i];
        }
        workSheet.Cells[2, 1].Value = facultyText;

        workSheet.Cells[3, 1].Value = $"Exported at {DateTime.Now:F}";

        workSheet.Cells[7, 1].LoadFromCollection(data.Modules, opt =>
        {
            opt.PrintHeaders = true;
            opt.TableStyle = OfficeOpenXml.Table.TableStyles.Light1;
            opt.HeaderParsingType = OfficeOpenXml.LoadFunctions.Params.HeaderParsingTypes.CamelCaseToSpace;
            opt.Members = new MemberInfo[]
            {
                completion.GetProperty(nameof(CompletionRecordExtendedDetailsDto.ModuleName)),
                completion.GetProperty(nameof(CompletionRecordExtendedDetailsDto.ModuleFrequency)),
                completion.GetProperty(nameof(CompletionRecordExtendedDetailsDto.TimeToExpiry)),
                completion.GetProperty(nameof(CompletionRecordExtendedDetailsDto.RecordEffectiveDate)),
                completion.GetProperty(nameof(CompletionRecordExtendedDetailsDto.DueDate))
            };
        });

        workSheet.Cells[7, 4, workSheet.Dimension.Rows, 5].Style.Numberformat.Format = "dd/MM/yyyy";

        // Highlight overdue entries
        ExcelAddress dataRange = new(8, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns);

        IExcelConditionalFormattingExpression formatNeverCompleted = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatNeverCompleted.Formula = "=$C8 = -9999";
        formatNeverCompleted.Style.Fill.BackgroundColor.Color = Color.Gray;
        formatNeverCompleted.Style.Font.Color.Color = Color.White;
        formatNeverCompleted.StopIfTrue = true;

        IExcelConditionalFormattingExpression formatOverdue = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatOverdue.Formula = "=$C8 < 1";
        formatOverdue.Style.Fill.BackgroundColor.Color = Color.Red;
        formatOverdue.Style.Font.Color.Color = Color.White;
        formatOverdue.StopIfTrue = true;

        IExcelConditionalFormattingExpression formatSoonExpire = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatSoonExpire.Formula = "=$C8 < 14";
        formatSoonExpire.Style.Fill.BackgroundColor.Color = Color.Yellow;
        formatSoonExpire.StopIfTrue = true;

        // Add format legend
        workSheet.Cells[5, 2].Value = "Colour Legend";
        workSheet.Cells[5, 2].Style.Font.Bold = true;
        workSheet.Cells[5, 3].Value = "Expired";
        workSheet.Cells[5, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
        workSheet.Cells[5, 3].Style.Fill.BackgroundColor.SetColor(Color.Red);
        workSheet.Cells[5, 3].Style.Font.Color.SetColor(Color.White);
        workSheet.Cells[5, 4].Value = "Expiring Soon";
        workSheet.Cells[5, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
        workSheet.Cells[5, 4].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
        workSheet.Cells[5, 5].Value = "Never Completed";
        workSheet.Cells[5, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
        workSheet.Cells[5, 5].Style.Fill.BackgroundColor.SetColor(Color.Gray);
        workSheet.Cells[5, 5].Style.Font.Color.SetColor(Color.White);
        workSheet.Cells[5, 2, 5, 5].Style.Border.BorderAround(ExcelBorderStyle.Thick);
        workSheet.Cells[5, 3, 5, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // Freeze top rows
        workSheet.View.FreezePanes(8, 1);
        workSheet.Cells[5, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public List<TrainingModule> ImportMandatoryTrainingDataFromFile(MemoryStream excelFile)
    {
        ExcelPackage excel = new(excelFile);
        ExcelWorksheet workSheet = excel.Workbook.Worksheets[0];

        int numModules = workSheet.Dimension.Rows;

        List<TrainingModule> modules = new();

        for (int row = 2; row <= numModules; row++)
        {
            TrainingModule entry = TrainingModule.Create(
                workSheet.Cells[row, 1].GetCellValue<string>(),
                (TrainingModuleExpiryFrequency)workSheet.Cells[row, 2].GetCellValue<int>(),
                workSheet.Cells[row, 3].GetCellValue<string>());

            modules.Add(entry);
        }

        int numStaff = workSheet.Dimension.Columns;

        for (int column = 5; column <= numStaff; column++)
        {
            for (int row = 2; row <= numModules; row++)
            {
                string completed = workSheet.Cells[row, column].GetCellValue<string>();
                if (string.IsNullOrWhiteSpace(completed))
                    continue;

                DateOnly dateCompleted = DateOnly.Parse(completed);

                string moduleName = workSheet.Cells[row, 1].GetCellValue<string>();
                TrainingModule module = modules.FirstOrDefault(entry => entry.Name == moduleName);
                if (module is null)
                    continue;

                TrainingCompletion entry = TrainingCompletion.Create(
                    workSheet.Cells[1, column].GetCellValue<string>(),
                    module.Id,
                    dateCompleted);

                module.AddCompletion(entry);
            }
        }

        excel.Dispose();

        return modules;
    }

    public async Task<Result<MemoryStream>> CreateGroupTutorialAttendanceFile(TutorialDetailsDto data)
    {
        ExcelPackage excel = new();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Sheet 1");

        if (data is null)
        {
            return Result.Failure<MemoryStream>(new("Application.Error", "No data provided"));
        }

        ExcelRichText nameDetail = workSheet.Cells[1, 1].RichText.Add(data.Name);
        nameDetail.Bold = true;
        nameDetail.Size = 16;

        ExcelRichText dateDetail = workSheet.Cells[2, 1].RichText.Add($"From {data.StartDate.ToShortDateString()} to {data.EndDate.ToShortDateString()}");
        dateDetail.Bold = true;
        dateDetail.Size = 16;

        workSheet.Cells[4, 1].RichText.Add("Not Enrolled");
        workSheet.Cells[4, 2].RichText.Add("-");
        workSheet.Cells[4, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
        workSheet.Cells[4, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

        workSheet.Cells[5, 1].RichText.Add("Enrolled, Not Present");
        workSheet.Cells[5, 2].RichText.Add("N");
        workSheet.Cells[5, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
        workSheet.Cells[5, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

        workSheet.Cells[6, 1].RichText.Add("Enrolled, Present");
        workSheet.Cells[6, 2].RichText.Add("Y");
        workSheet.Cells[6, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
        workSheet.Cells[6, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

        List<IGrouping<StudentId, TutorialRollStudentDetailsDto>> students = data.Rolls
            .SelectMany(roll => roll.Students)
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.Name)
            .GroupBy(student => student.StudentId)
            .ToList();

        List<TutorialRollDetailsDto> rolls = data.Rolls.OrderBy(roll => roll.SessionDate).ToList();

        int startColumn = 3;
        int startRow = 8;

        for (int i = 0; i < rolls.Count; i++)
        {
            workSheet.Cells[startRow, startColumn + i].Value = rolls[i].SessionDate.ToShortDateString();
            workSheet.Cells[startRow, startColumn + i].Style.Numberformat.Format = "dd/MM/yyyy";

            workSheet.Cells[startRow + 1, startColumn + i].Value = rolls[i].StaffName;
        }

        startRow += 2;
        startColumn -= 2;

        for (int i = 0; i < students.Count; i++)
        {
            IGrouping<StudentId, TutorialRollStudentDetailsDto> student = students[i];

            workSheet.Cells[startRow + i, startColumn].Value = student.First().Name;
            workSheet.Cells[startRow + i, startColumn + 1].Value = student.First().Grade;

            for(int j = 0; j < rolls.Count; j++)
            {
                string text;
                bool enrolled = false;

                TutorialRollStudentDetailsDto entry = rolls[j].Students.FirstOrDefault(innerStudent => innerStudent.StudentId == students[i].Key);

                if (entry is null)
                {
                    // Student was not included in this roll
                    text = "-";
                }
                else
                {
                    text = (entry.Present ? "Y" : "N");
                    enrolled = entry.Enrolled;
                }

                workSheet.Cells[startRow + i, startColumn + 2 + j].Value = text;
                workSheet.Cells[startRow + i, startColumn + 2 + j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                if (enrolled)
                {
                    workSheet.Cells[startRow + i, startColumn + 2 + j].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[startRow + i, startColumn + 2 + j].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                } 
                else
                {
                    workSheet.Cells[startRow + i, startColumn + 2 + j].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[startRow + i, startColumn + 2 + j].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }
            }
        }

        workSheet.View.FreezePanes(10, 3);
        workSheet.Cells[4, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateFamilyContactDetailsChangeReport(
        List<ParentContactChangeDto> changes, 
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Sheet 1");

        ExcelRichText nameDetail = workSheet.Cells[1, 1].RichText.Add("Parent Contact Details Changed");
        nameDetail.Bold = true;
        nameDetail.Size = 16;

        ExcelRichText dateDetail = workSheet.Cells[2, 1].RichText.Add($"Report generated on {DateTime.Today.ToLongDateString()}");
        dateDetail.Bold = true;
        dateDetail.Size = 16;

        workSheet.Cells[4, 1].LoadFromCollection(changes, true);

        workSheet.View.FreezePanes(5, 1);
        workSheet.Cells[4, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFilter = true;
        workSheet.Cells[5, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public Task<List<MasterFileSchool>> GetSchoolsFromMasterFile(MemoryStream stream)
    {
        List<MasterFileSchool> schoolsList = new();

        using ExcelPackage package = new(stream);
        ExcelWorksheet worksheet = package.Workbook.Worksheets.First(sheet => sheet.Name == "Partner_schools");
        int rows = worksheet.Dimension.Rows;

        for (int i = 2; i <= rows; i++)
        {
            object codeCell = worksheet.Cells[i, 9].Value;
            if (codeCell is null)
                continue;

            string code = codeCell.ToString()?.Trim() ?? string.Empty;

            object nameCell = worksheet.Cells[i, 1].Value;
            if (nameCell is null)
                continue;

            string name = ((string)nameCell).Trim();

            object statusCell = worksheet.Cells[i, 2].Value;
            if (statusCell is null)
                continue;

            SiteStatus siteStatus = statusCell.ToString() switch
            {
                "*INACTIVE*" => SiteStatus.Inactive,
                "Student(s)" => SiteStatus.Students,
                "Teacher(s)" => SiteStatus.Teachers,
                "Student(s) and Teacher(s)" => SiteStatus.Both,
                _ => SiteStatus.Unknown
            };

            object principalCell = worksheet.Cells[i, 21].Value;
            string principal = (principalCell is not null && principalCell is not ExcelErrorValue) ? ((string)principalCell).Trim() : string.Empty;

            object principalEmailCell = worksheet.Cells[i, 22].Value;
            string principalEmail = string.Empty;
            
            if (principalEmailCell is not null &&
                principalEmailCell is not ExcelErrorValue &&
                principalEmailCell.GetType() != typeof(double))
            {
                principalEmail = principalEmailCell.ToString()!.Trim();
            }

            schoolsList.Add(new MasterFileSchool(
                i,
                code,
                name,
                siteStatus,
                principal,
                principalEmail));
        }

        return Task.FromResult(schoolsList);
    }

    public Task<List<MasterFileStudent>> GetStudentsFromMasterFile(MemoryStream stream)
    {
        List<MasterFileStudent> studentList = new();

        using ExcelPackage package = new(stream);
        ExcelWorksheet worksheet = package.Workbook.Worksheets.First(sheet => sheet.Name == "Students");
        int rows = worksheet.Dimension.Rows;

        for (int i = 2; i <= rows; i++)
        {
            object srnCell = worksheet.Cells[i, 1].Value;
            if (srnCell is null)
                continue;

            string srn = srnCell.ToString()?.Trim() ?? string.Empty;

            object fNameCell = worksheet.Cells[i, 3].Value;
            if (fNameCell is null)
                continue;

            string fName = ((string)fNameCell).Trim();

            object sNameCell = worksheet.Cells[i, 4].Value;
            if (sNameCell is null)
                continue;

            string sName = ((string)sNameCell).Trim();

            object gradeCell = worksheet.Cells[i, 7].Value;
            if (gradeCell is null)
                continue;

            Grade grade = gradeCell.ToString()?.Trim() switch
            {
                "5" => Grade.Y05,
                "6" => Grade.Y06,
                "6*" => Grade.Y06,
                "7" => Grade.Y07,
                "7*" => Grade.Y07,
                "8" => Grade.Y08,
                "9" => Grade.Y09,
                "10" => Grade.Y10,
                "11" => Grade.Y11,
                "12" => Grade.Y12,
                _ => Grade.SpecialProgram
            };

            string parent1Cell = worksheet.Cells[i, 39].Value as string;
            string parent1 = string.Empty;
            if (parent1Cell is not null && !string.IsNullOrWhiteSpace(parent1Cell))
                parent1 = parent1Cell.Trim();

            string parent2Cell = worksheet.Cells[i, 40].Value as string;
            string parent2 = string.Empty;
            if (parent2Cell is not null && !string.IsNullOrWhiteSpace(parent2Cell))
                parent2 = parent2Cell.Trim();

            studentList.Add(new MasterFileStudent(
                i,
                srn,
                fName,
                sName,
                grade,
                parent1,
                parent2));
        }

        return Task.FromResult(studentList);
    }

    public async Task<MemoryStream> CreateMasterFileConsistencyReport(
        List<UpdateItem> updateItems, 
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Sheet 1");

        ExcelRichText nameDetail = workSheet.Cells[1, 1].RichText.Add("MasterFile Consistency Report");
        nameDetail.Bold = true;
        nameDetail.Size = 16;

        workSheet.Cells[3, 1].LoadFromCollection(updateItems, true);

        workSheet.View.FreezePanes(4, 1);
        workSheet.Cells[3, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFilter = true;
        workSheet.Cells[4, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateContactExportFile(
        List<ContactResponse> contacts, 
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Contacts");

        workSheet.Cells[1, 1].LoadFromCollection(contacts, opt =>
        {
            opt.PrintHeaders = true;
            opt.HeaderParsingType = OfficeOpenXml.LoadFunctions.Params.HeaderParsingTypes.CamelCaseToSpace;
        });

        workSheet.View.FreezePanes(2, 1);
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFilter = true;
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateAwardNominationsExportFileByStudent(
        List<AwardNominationExportByStudentDto> nominations,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Nominations");

        // TODO: R1.16: Ensure that code is rebased from master once this action has been completed and before publishing

        workSheet.Cells[1, 1].Value = "SRN";
        workSheet.Cells[1, 2].Value = "Student First Name";
        workSheet.Cells[1, 3].Value = "Student Last Name";
        workSheet.Cells[1, 4].Value = "Student Name";
        workSheet.Cells[1, 5].Value = "Grade";
        workSheet.Cells[1, 6].Value = "School";
        workSheet.Cells[1, 7].Value = "Awards";
        
        int totalRows = nominations.Count;

        for (int row = 2; row <= totalRows + 1; row++)
        {
            workSheet.Cells[row, 1].Value = nominations[row -2].SRN;
            workSheet.Cells[row, 2].Value = nominations[row -2].StudentName.PreferredName;
            workSheet.Cells[row, 3].Value = nominations[row -2].StudentName.LastName;
            workSheet.Cells[row, 4].Value = nominations[row -2].StudentName.DisplayName;
            workSheet.Cells[row, 5].Value = nominations[row -2].Grade.AsName();
            workSheet.Cells[row, 6].Value = nominations[row -2].School;

            foreach (string award in nominations[row - 2].Awards)
            {
                if (nominations[row - 2].Awards.IndexOf(award) != 0)
                    workSheet.Cells[row, 7].RichText.Add("\r\n");

                workSheet.Cells[row, 7].RichText.Add($"\u2022 {award.Trim()}");
            }
        }

        workSheet.View.FreezePanes(2, 1);
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFilter = true;
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();
        workSheet.Columns[7].Style.WrapText = true;
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateAwardNominationsExportFileBySchool(
        List<AwardNominationExportBySchoolDto> nominations,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Nominations");
        
        workSheet.Cells[1, 1].Value = "School";
        workSheet.Cells[1, 2].Value = "Awards";

        int totalRows = nominations.Count;

        for (int row = 2; row <= totalRows + 1; row++)
        {
            workSheet.Cells[row, 1].Value = nominations[row - 2].School;

            foreach (AwardNominationExportByStudentDto student in nominations[row - 2].Students)
            {
                if (nominations[row - 2].Students.IndexOf(student) != 0)
                {
                    workSheet.Cells[row, 2].RichText.Add("\r\n");
                    workSheet.Cells[row, 2].RichText.Add("\r\n");
                }

                workSheet.Cells[row, 2].RichText.Add($"{student.StudentName.DisplayName} ({student.Grade.AsName()})");

                foreach (string award in student.Awards)
                {
                    workSheet.Cells[row, 2].RichText.Add("\r\n");
                    workSheet.Cells[row, 2].RichText.Add($"  \u2022 {award.Trim()}");
                }
            }
        }

        workSheet.View.FreezePanes(2, 1);
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFilter = true;
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();
        workSheet.Columns[2].Style.WrapText = true;
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateAwardNominationsExportFileBySubject(
        List<AwardNominationExportBySubjectDto> nominations,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Nominations");

        workSheet.Cells[1, 1].Value = "Subject";
        workSheet.Cells[1, 2].Value = "Awards";

        int totalRows = nominations.Count;

        for (int row = 2; row <= totalRows + 1; row++)
        {
            workSheet.Cells[row, 1].Value = nominations[row - 2].Subject;

            foreach (AwardNominationExportByStudentDto student in nominations[row - 2].Students)
            {
                if (nominations[row - 2].Students.IndexOf(student) != 0)
                {
                    workSheet.Cells[row, 2].RichText.Add("\r\n");
                    workSheet.Cells[row, 2].RichText.Add("\r\n");
                }

                workSheet.Cells[row, 2].RichText.Add($"{student.StudentName.DisplayName} ({student.Grade.AsName()})");

                foreach (string award in student.Awards)
                {
                    workSheet.Cells[row, 2].RichText.Add("\r\n");
                    workSheet.Cells[row, 2].RichText.Add($"  \u2022 {award.Trim()}");
                }
            }
        }

        workSheet.View.FreezePanes(2, 1);
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFilter = true;
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();
        workSheet.Columns[2].Style.WrapText = true;
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateAwardNominationsExportFile(
        List<AwardNominationExportDto> nominations,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Nominations");
        
        workSheet.Cells[1, 1].Value = "SRN";
        workSheet.Cells[1, 2].Value = "Student First Name";
        workSheet.Cells[1, 3].Value = "Student Last Name";
        workSheet.Cells[1, 4].Value = "Student Name";
        workSheet.Cells[1, 5].Value = "Grade";
        workSheet.Cells[1, 6].Value = "School";
        workSheet.Cells[1, 7].Value = "Awards";

        int totalRows = nominations.Count;

        for (int row = 2; row <= totalRows + 1; row++)
        {
            workSheet.Cells[row, 1].Value = nominations[row - 2].SRN;
            workSheet.Cells[row, 2].Value = nominations[row - 2].StudentName.PreferredName;
            workSheet.Cells[row, 3].Value = nominations[row - 2].StudentName.LastName;
            workSheet.Cells[row, 4].Value = nominations[row - 2].StudentName.DisplayName;
            workSheet.Cells[row, 5].Value = nominations[row - 2].Grade.AsName();
            workSheet.Cells[row, 6].Value = nominations[row - 2].School;
            workSheet.Cells[row, 7].Value = nominations[row - 2].Award;
        }

        workSheet.View.FreezePanes(2, 1);
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFilter = true;
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public List<StudentAttendanceData> ExtractPerDayYearToDateAttendanceData(
        SystemAttendanceData systemData, 
        List<StudentAttendanceData> data)
    {
        if (systemData?.YearToDateDayCalculationDocument is null)
            return data;

        data ??= new();

        List<string> ytdDayData = systemData.YearToDateDayCalculationDocument.DocumentNode.InnerHtml.Split('\u000A').ToList();

        // Remove first and last entry
        ytdDayData.RemoveAt(0);
        ytdDayData.RemoveAt(ytdDayData.Count - 1);

        foreach (string row in ytdDayData)
        {
            string[] line = _csvParser.Split(row);

            // Index 0: Surname
            // Index 1: Preferred name
            // Index 2: Gender
            // Index 3: Roll class name
            // Index 4: School year
            // Index 5: External id
            // Index 6: Suburb
            // Index 7: Days total
            // Index 8: Today's attendance reason
            // Index 9: Days absent
            // Index 10: Days attended
            // Index 11: Percentage attendance
            // Index 12: Percentage absent
            // Index 13: Explained absences
            // Index 14: Unexplained absences
            // Index 15: Percentage explained
            // Index 16: Percentage unexplained

            string srn = line[5].FormatField();

            Result<StudentReferenceNumber> studentReferenceNumber = StudentReferenceNumber.Create(srn);

            if (studentReferenceNumber.IsFailure)
                continue;

            StudentAttendanceData entry = data.FirstOrDefault(entry => entry.StudentReferenceNumber == studentReferenceNumber.Value);

            if (entry is not null)
            {
                entry.DayYTD = Convert.ToDecimal(line[11].FormatField(), null);
            }
            else
            {
                entry = new()
                {
                    StudentReferenceNumber = studentReferenceNumber.Value,
                    Name = $"{line[1].FormatField()} {line[0].FormatField()}",
                    Grade = (Grade)Convert.ToInt32(line[4].FormatField(), null),
                    DayYTD = Convert.ToDecimal(line[11].FormatField(), null)
                };

                data.Add(entry);
            }
        }

        return data;
    }

    public List<StudentAttendanceData> ExtractPerMinuteYearToDateAttendanceData(
        SystemAttendanceData systemData, 
        List<StudentAttendanceData> data)
    {
        if (systemData?.YearToDateMinuteCalculationDocument is null)
            return data;

        data ??= new();

        using IExcelDataReader reader = ExcelReaderFactory.CreateReader(systemData.YearToDateMinuteCalculationDocument);
        DataSet result = reader.AsDataSet();

        foreach (DataRow row in result.Tables[0].Rows)
        {
            if (row.ItemArray.First()?.ToString() == "Student ID") // This is a header row
                continue;

            if (row[6].ToString() != "Overall") // This is a class specific value
                continue;

            // Index 0: Student ID
            // Index 1: First Name
            // Index 2: Last Name
            // Index 3: Gender
            // Index 4: School Year
            // Index 5: Roll Class
            // Index 6: Class
            // Index 7: Class Time
            // Index 8: Absence Time
            // Index 9: Un-tallied Time
            // Index 10: Percentage

            string srn = row[0].ToString();

            Result<StudentReferenceNumber> studentReferenceNumber = StudentReferenceNumber.Create(srn);

            if (studentReferenceNumber.IsFailure)
                continue;

            StudentAttendanceData entry = data.FirstOrDefault(entry => entry.StudentReferenceNumber == studentReferenceNumber.Value);

            if (entry is not null)
            {
                entry.MinuteYTD = Convert.ToDecimal(row[10], null);
            }
            else
            {
                entry = new()
                {
                    StudentReferenceNumber = studentReferenceNumber.Value,
                    Name = $"{row[1].ToString().FormatField()} {row[2].ToString().FormatField()}",
                    MinuteYTD = Convert.ToDecimal(row[10], null)
                };

                data.Add(entry);
            }
        }

        return data;
    }

    public List<StudentAttendanceData> ExtractPerDayWeekAttendanceData(
        SystemAttendanceData systemData, 
        List<StudentAttendanceData> data)
    {
        if (systemData?.WeekDayCalculationDocument is null)
            return data;

        data ??= new();

        List<string> fnDayData = systemData.WeekDayCalculationDocument.DocumentNode.InnerHtml.Split('\u000A').ToList();

        // Remove first and last entry
        fnDayData.RemoveAt(0);
        fnDayData.RemoveAt(fnDayData.Count - 1);

        foreach (string row in fnDayData)
        {
            string[] line = _csvParser.Split(row);

            // Index 0: Surname
            // Index 1: Preferred name
            // Index 2: Gender
            // Index 3: Roll class name
            // Index 4: School year
            // Index 5: External id
            // Index 6: Suburb
            // Index 7: Days total
            // Index 8: Today's attendance reason
            // Index 9: Days absent
            // Index 10: Days attended
            // Index 11: Percentage attendance
            // Index 12: Percentage absent
            // Index 13: Explained absences
            // Index 14: Unexplained absences
            // Index 15: Percentage explained
            // Index 16: Percentage unexplained

            string srn = line[5].FormatField();

            Result<StudentReferenceNumber> studentReferenceNumber = StudentReferenceNumber.Create(srn);

            if (studentReferenceNumber.IsFailure)
                continue;

            StudentAttendanceData entry = data.FirstOrDefault(entry => entry.StudentReferenceNumber == studentReferenceNumber.Value);

            if (entry is not null)
            {
                string value = line[11].FormatField();

                if (string.IsNullOrWhiteSpace(value))
                {
                    entry.DayWeek = 0;

                    continue;
                }

                entry.DayWeek = Convert.ToDecimal(value, null);
            }
            else
            {
                string value = line[11].FormatField();

                decimal dayWeekValue = 0;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    dayWeekValue = Convert.ToDecimal(value, null);
                }

                entry = new()
                {
                    StudentReferenceNumber = studentReferenceNumber.Value,
                    Name = $"{line[1].FormatField()} {line[0].FormatField()}",
                    DayWeek = dayWeekValue
                };

                data.Add(entry);
            }
        }

        return data;
    }

    public List<StudentAttendanceData> ExtractPerMinuteWeekAttendanceData(
        SystemAttendanceData systemData, 
        List<StudentAttendanceData> data)
    {
        if (systemData?.WeekMinuteCalculationDocument is null)
            return data;

        data ??= new();

        using IExcelDataReader reader = ExcelReaderFactory.CreateReader(systemData.WeekMinuteCalculationDocument);
        DataSet result = reader.AsDataSet();

        foreach (DataRow row in result.Tables[0].Rows)
        {
            if (row.ItemArray.First()?.ToString() == "Student ID") // This is a header row
                continue;

            if (row[6].ToString() != "Overall") // This is a class specific value
                continue;

            // Index 0: Student ID
            // Index 1: First Name
            // Index 2: Last Name
            // Index 3: Gender
            // Index 4: School Year
            // Index 5: Roll Class
            // Index 6: Class
            // Index 7: Class Time
            // Index 8: Absence Time
            // Index 9: Un-tallied Time
            // Index 10: Percentage

            string srn = row[0].ToString();

            Result<StudentReferenceNumber> studentReferenceNumber = StudentReferenceNumber.Create(srn);

            if (studentReferenceNumber.IsFailure)
                continue;

            StudentAttendanceData entry = data.FirstOrDefault(entry => entry.StudentReferenceNumber == studentReferenceNumber.Value);

            if (entry is not null)
            {
                entry.MinuteWeek = Convert.ToDecimal(row[10], null);
            }
            else
            {
                entry = new()
                {
                    StudentReferenceNumber = studentReferenceNumber.Value,
                    Name = $"{row[1].ToString().FormatField()} {row[2].ToString().FormatField()}",
                    MinuteWeek = Convert.ToDecimal(row[10], null)
                };

                data.Add(entry);
            }
        }

        return data;
    }

    public Task<List<SentralIncidentDetails>> ConvertSentralIncidentReport(
        Stream baseFile, 
        Stream detailFile,
        List<DateOnly> excludedDates, 
        CancellationToken cancellationToken = default)
    {
        List<SentralIncidentDetails> response = new();

        if (detailFile is null || baseFile is null)
            return Task.FromResult(response);

        using IExcelDataReader detailReader = ExcelReaderFactory.CreateReader(detailFile);
        DataSet detailResult = detailReader.AsDataSet(new ExcelDataSetConfiguration { UseColumnDataType = true });
        
        using IExcelDataReader baseReader = ExcelReaderFactory.CreateReader(baseFile);
        DataSet baseResult = baseReader.AsDataSet(new ExcelDataSetConfiguration { UseColumnDataType = true });

        foreach (DataRow row in detailResult.Tables[0].Rows)
        {
            if (row.ItemArray.First()?.ToString() == "Student Id") // This is a header row
                continue;

            // baseFile
            // Index 0: Student Id
            // Index 1: Confidential
            // Index 2: Date Created
            // Index 3: Date of Incident
            // Index 4: Day
            // Index 5: Incident
            // Index 6: Student Was
            // Index 7: Incident Time
            // Index 8: Period
            // Index 9: Subject
            // Index 10: Category
            // Index 11: Type
            // Index 12: Sub Type
            // Index 13: Incident Records Description
            // Index 14: Incident Record Details
            // Index 15: Incident Record Detail Options
            // Index 16: Follow Up Action Comment
            // Index 17: Follow Up Actions
            // Index 18: Follow Up Action Status
            // Index 19: Teacher
            // Index 20: Student Surname,
            // Index 21: Student First Name,
            // Index 22: DOB
            // Index 23: Years
            // Index 24: Months
            // Index 25: School Year
            // Index 26: House
            // Index 27: Roll Class
            // Index 28: Location

            // detailFile
            // Index 0: Student Id
            // Index 1: Given Name
            // Index 2: Surname
            // Index 3: Year
            // Index 4: RollClass
            // Index 5: Incident #
            // Index 6: Date
            // Index 7: Incident Record Description
            // Index 8: Incident Record Details
            // Index 9: Subject
            // Index 10: Faculty
            // Index 11: Type
            // Index 12: Status
            // Index 13: Task Name / Course Requirement
            // Index 14: Initial Due Date
            // Index 15: Required Student Actions
            // Index 16: New Due Date

            string incidentId = row[5].ToString().FormatField();

            if (response.Any(entry => entry.IncidentId == incidentId))
                continue;

            string srn = row[0].ToString();

            bool dateExtractionSucceeded = DateOnly.TryParse(row[6].ToString(), out DateOnly dateCreated);

            if (!dateExtractionSucceeded)
                continue;
            
            List<DateOnly> datesBetween = dateCreated.Range(_dateTime.Today);
            datesBetween = datesBetween
                .Where(entry => !excludedDates.Contains(entry))
                .Where(entry => 
                    entry.DayOfWeek != DayOfWeek.Saturday && 
                    entry.DayOfWeek != DayOfWeek.Sunday)
                .ToList();

            int severity = datesBetween.Count - 1;

            int gradeNum = Convert.ToInt32(row[3], null);
            Grade grade = (Grade)gradeNum;

            DataRow? matchingRow = baseResult.Tables[0].Select($"Column5 = '{incidentId}'").FirstOrDefault();

            if (matchingRow is null)
                continue;

            response.Add(new(
                srn,
                dateCreated,
                row[5].ToString().FormatField(),
                row[9].ToString().FormatField(),
                row[11].ToString().FormatField(),
                matchingRow[19].ToString().FormatField(),
                row[1].ToString().FormatField(),
                row[2].ToString().FormatField(),
                grade,
                severity));
        }

        return Task.FromResult(response);
    }

    public async Task<MemoryStream> CreateWellbeingExportFile(
        List<SentralIncidentDetails> records,
        CancellationToken cancellationToken = default)
    {
        List<IncidentRow> rows = records
            .Select(entry =>
                new IncidentRow()
                {
                    Age = entry.Severity,
                    Date = entry.DateCreated,
                    Subject = entry.Subject,
                    Type = entry.Type,
                    Teacher = entry.Teacher,
                    Surname = entry.StudentLastName,
                    Name = entry.StudentFirstName,
                    Year = entry.Grade.AsName()
                })
            .OrderBy(entry => entry.Age)
            .ToList();

        ExcelPackage excel = new();

        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Sheet 1");

        ExcelRangeBase table = worksheet.Cells[1, 1].LoadFromCollection(rows, true);
        worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns].Style.Font.Bold = true;
        worksheet.Cells[2, 2, worksheet.Dimension.Rows, worksheet.Dimension.Columns].Style.Numberformat.Format = "dd/MM/yyyy";

        ExcelRangeBase data = worksheet.Cells[2, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns];

        worksheet.View.FreezePanes(2, 1);

        table.AutoFilter = true;
        table.AutoFitColumns();

        IExcelConditionalFormattingExpression bandFourFormat = worksheet.ConditionalFormatting.AddExpression(data);
        bandFourFormat.Formula = "=$A2 > 24";
        bandFourFormat.Style.Fill.PatternType = ExcelFillStyle.Solid;
        bandFourFormat.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 196, 89, 17));
        bandFourFormat.Style.Font.Color.SetColor(Color.White);
        bandFourFormat.StopIfTrue = true;

        IExcelConditionalFormattingExpression bandThreeFormat = worksheet.ConditionalFormatting.AddExpression(data);
        bandThreeFormat.Formula = "=$A2 > 19";
        bandThreeFormat.Style.Fill.PatternType = ExcelFillStyle.Solid;
        bandThreeFormat.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 244, 176, 131));
        bandThreeFormat.StopIfTrue = true;

        IExcelConditionalFormattingExpression bandTwoFormat = worksheet.ConditionalFormatting.AddExpression(data);
        bandTwoFormat.Formula = "=$A2 > 14";
        bandTwoFormat.Style.Fill.PatternType = ExcelFillStyle.Solid;
        bandTwoFormat.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 241, 202, 172));
        bandTwoFormat.StopIfTrue = true;

        IExcelConditionalFormattingExpression bandOneFormat = worksheet.ConditionalFormatting.AddExpression(data);
        bandOneFormat.Formula = "=$A2 > 9";
        bandOneFormat.Style.Fill.PatternType = ExcelFillStyle.Solid;
        bandOneFormat.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 251, 228, 213));
        bandOneFormat.StopIfTrue = true;

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateSciencePracOverdueReport(
        List<OverdueRollResponse> records,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();

        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Overdue Lesson Rolls");
        worksheet.Cells[1, 1].LoadFromCollection(records, opt =>
        {
            opt.HeaderParsingType = OfficeOpenXml.LoadFunctions.Params.HeaderParsingTypes.CamelCaseToSpace;
            opt.PrintHeaders = true;
        });
        worksheet.Cells[1, 4, worksheet.Dimension.Rows, 4].Style.Numberformat.Format = "dd/MM/yyyy";

        worksheet.View.FreezePanes(2, 1);
        worksheet.Cells[1, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns].AutoFitColumns();
        worksheet.Cells[1, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns].AutoFilter = true;

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateTrainingModuleOverallReportFile(
        List<ModuleDetails> moduleDetails,
        List<StaffStatus> staffStatuses,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();

        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Sheet 1");

        worksheet.Cells[2, 1].Value = "Staff Id";
        worksheet.Cells[2, 1].Style.Font.Bold = true;
        worksheet.Cells[2, 1].Style.Font.Size = 14;
        worksheet.Cells[2, 2].Value = "Name";
        worksheet.Cells[2, 2].Style.Font.Bold = true;
        worksheet.Cells[2, 2].Style.Font.Size = 14;
        worksheet.Cells[2, 3].Value = "School";
        worksheet.Cells[2, 3].Style.Font.Bold = true;
        worksheet.Cells[2, 3].Style.Font.Size = 14;
        worksheet.Cells[2, 4].Value = "Faculties";
        worksheet.Cells[2, 4].Style.Font.Bold = true;
        worksheet.Cells[2, 4].Style.Font.Size = 14;

        staffStatuses ??= new();
        moduleDetails ??= new();

        for (int row = 3; row < staffStatuses.Count + 3; row++)
        {
            worksheet.Cells[row, 1].Value = staffStatuses[row - 3].StaffId;
            worksheet.Cells[row, 2].Value = staffStatuses[row - 3].Name.DisplayName;
            worksheet.Cells[row, 3].Value = staffStatuses[row - 3].School;

            string facultiesList = string.Join("; ", staffStatuses[row - 3].Faculties);
            worksheet.Cells[row, 4].Value = facultiesList;
        }

        for (int col = 5; col < moduleDetails.Count + 5; col++)
        {
            ModuleDetails module = moduleDetails[col - 5];

            worksheet.Cells[1, col].Value = module.Name;
            worksheet.Cells[1, col].Style.TextRotation = 90;
            worksheet.Cells[1, col].Style.Font.Bold = true;
            worksheet.Cells[1, col].Style.Font.Size = 14;
            worksheet.Cells[2, col].Value = module.Expiry.GetDisplayName();
            worksheet.Cells[2, col].Style.Font.Bold = true;
            worksheet.Cells[2, col].Style.Font.Size = 14;

            for (int row = 3; row < staffStatuses.Count + 3; row++)
            {
                ModuleStatus status = staffStatuses[row - 3].Modules.FirstOrDefault(entry => entry.ModuleId == module.ModuleId);

                if (status is null)
                    continue;

                DateOnly nextDueDate = status.LastCompletionDate?.AddYears((int)module.Expiry) ?? DateOnly.MinValue;
                
                worksheet.Cells[row, col].Value = status.LastCompletionDate.HasValue ? status.LastCompletionDate.Value.ToDateTime(TimeOnly.MaxValue) : "-";
                worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, col].Style.Numberformat.Format = "dd/MM/yyyy";

                if (module.Expiry == TrainingModuleExpiryFrequency.OnceOff)
                {
                    if (status.LastCompletionDate.HasValue)
                    {
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.Green);
                        worksheet.Cells[row, col].Style.Font.Color.SetAuto();
                    }
                    else
                    {
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.White);
                    }

                    continue;
                } 
                
                if (!status.Required)
                {
                    worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.DarkGray);
                    continue;
                }

                if (status.LastCompletionDate.HasValue && nextDueDate < _dateTime.Today || !status.LastCompletionDate.HasValue)
                {
                    worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                    worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.White);
                    continue;
                }

                if (status.LastCompletionDate.HasValue && nextDueDate.AddDays(-30) < _dateTime.Today)
                {
                    worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.Orange);
                    worksheet.Cells[row, col].Style.Font.Color.SetAuto();
                    continue;
                }

                worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.Green);
                worksheet.Cells[row, col].Style.Font.Color.SetAuto();
            }
        }

        worksheet.Cells[2, 1, 2, worksheet.Dimension.Columns].AutoFilter = true;
        worksheet.Cells[1, 1, worksheet.Dimension.Rows, 4].AutoFitColumns();
        worksheet.Cells[1, 5, worksheet.Dimension.Rows, worksheet.Dimension.Columns].EntireColumn.Width = 16;
        worksheet.View.FreezePanes(3, 1);

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateWorkFlowReport(
        List<CaseReportItem> records,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();

        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Report");

        worksheet.Cells[1, 1].Value = "Id";
        worksheet.Cells[1, 2].Value = "Student";
        worksheet.Cells[1, 3].Value = "Grade";
        worksheet.Cells[1, 4].Value = "Description";
        worksheet.Cells[1, 5].Value = "Created Date";
        worksheet.Cells[1, 6].Value = "Completed Date";
        worksheet.Cells[1, 7].Value = "Assigned To";
        worksheet.Cells[1, 8].Value = "Open Days";

        records ??= new();

        for (int row = 0; row < records.Count; row++)
        {
            CaseReportItem record = records[row];

            worksheet.Cells[row + 2, 1].Value = record.Id;
            worksheet.Cells[row + 2, 2].Value = record.Student.DisplayName;
            worksheet.Cells[row + 2, 3].Value = record.Grade.AsName();
            worksheet.Cells[row + 2, 4].Value = record.Description;
            worksheet.Cells[row + 2, 4].Style.Numberformat.Format = "dd/MM/yyyy";
            worksheet.Cells[row + 2, 5].Value = record.CreatedDate.ToDateTime(TimeOnly.MinValue);
            worksheet.Cells[row + 2, 5].Style.Numberformat.Format = "dd/MM/yyyy";
            worksheet.Cells[row + 2, 6].Value = record.CompletedDate?.ToDateTime(TimeOnly.MinValue);
            worksheet.Cells[row + 2, 7].Value = record.AssignedTo;
            worksheet.Cells[row + 2, 8].Value = record.OpenDays;
        }

        worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns].AutoFilter = true;
        worksheet.Cells[1, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns].AutoFitColumns();
        worksheet.View.FreezePanes(2, 1);
        
        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateSchoolContactExport(
        List<SchoolWithContactsResponse> records,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();

        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Contacts");

        worksheet.Cells[1, 1].Value = "School Code";
        worksheet.Cells[1, 2].Value = "School Type";
        worksheet.Cells[1, 3].Value = "School";
        worksheet.Cells[1, 4].Value = "Contact Name";
        worksheet.Cells[1, 5].Value = "Contact Email";
        worksheet.Cells[1, 6].Value = "Contact Phone";
        worksheet.Cells[1, 7].Value = "Contact Role";
        worksheet.Cells[1, 8].Value = "Notes";

        records ??= new();

        int row = 2;

        foreach (SchoolWithContactsResponse school in records)
        {
            if (school.Contacts.Count == 0)
            {
                worksheet.Cells[row, 1].Value = school.SchoolCode;
                worksheet.Cells[row, 2].Value = school.SchoolType.Name;
                worksheet.Cells[row, 3].Value = school.SchoolName;

                row++;
            }

            foreach (SchoolWithContactsResponse.ContactDetails contact in school.Contacts)
            {
                worksheet.Cells[row, 1].Value = school.SchoolCode;
                worksheet.Cells[row, 2].Value = school.SchoolType.Name;
                worksheet.Cells[row, 3].Value = school.SchoolName;
                worksheet.Cells[row, 4].Value = contact.Contact.DisplayName;
                worksheet.Cells[row, 5].Value = contact.EmailAddress.Email;
                worksheet.Cells[row, 6].Value = contact.PhoneNumber.ToString();
                worksheet.Cells[row, 7].Value = contact.Role;
                worksheet.Cells[row, 8].Value = contact.Note;

                row++;
            }
        }

        worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns].AutoFilter = true;
        worksheet.Cells[1, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns].AutoFitColumns();
        worksheet.View.FreezePanes(2, 1);

        ExcelWorksheet pivotSheet = excel.Workbook.Worksheets.Add("Pivot Table");
        ExcelPivotTable pivotTable = pivotSheet.PivotTables.Add(
            pivotSheet.Cells[1, 1],
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns],
            "Number of Contacts per School by Role");

        pivotTable.ColumnHeaderCaption = "Role";
        pivotTable.RowHeaderCaption = "School";

        pivotTable.RowFields.Add(pivotTable.Fields["School Type"]);
        
        ExcelPivotTableField schoolRowField = pivotTable.RowFields.Add(pivotTable.Fields["School"]);
        schoolRowField.Sort = eSortType.Ascending;
        
        pivotTable.ColumnFields.Add(pivotTable.Fields["Contact Role"]);
        
        ExcelPivotTableDataField countDataField = pivotTable.DataFields.Add(pivotTable.Fields["Contact Name"]);
        countDataField.Function = DataFieldFunctions.Count;
        countDataField.Name = "Number of Contacts per School by Role";

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateCanvasRubricResultExport(
        RubricEntry rubric,
        List<CourseEnrolmentEntry> enrolments,
        List<AssignmentResultEntry> results,
        List<Student> students,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();

        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Results");

        worksheet.Cells[1, 1].Value = "Student Id";
        worksheet.Cells[1, 2].Value = "Student First Name";
        worksheet.Cells[1, 3].Value = "Student Last Name";

        Dictionary<string, int> criteriaOrder = new();
        int column = 4;

        foreach (RubricEntry.RubricCriterion entry in rubric.Criteria)
        {
            criteriaOrder.Add(entry.CriterionId, column);

            worksheet.Cells[1, column].Value = $"{entry.Name} (Mark)";
            worksheet.Cells[1, column + 1].Value = $"{entry.Name} (Grade)";

            column += 2;
        }
        
        worksheet.Cells[1, column].Value = "Overall Mark";
        worksheet.Cells[1, column + 1].Value = "Overall Grade";
        int gradeColumn = column;

        int row = 2;

        foreach (CourseEnrolmentEntry enrolment in enrolments)
        {
            if (enrolment.Role != CourseEnrolmentEntry.EnrolmentRole.Student)
                continue;

            worksheet.Cells[row, 1].Value = enrolment.UserId;

            Result<StudentReferenceNumber> studentReferenceNumber = StudentReferenceNumber.Create(enrolment.UserId);

            if (studentReferenceNumber.IsFailure)
                continue;

            Student student = students.FirstOrDefault(entry => entry.StudentReferenceNumber == studentReferenceNumber.Value);

            if (student is not null)
            {
                worksheet.Cells[row, 2].Value = student.Name.PreferredName;
                worksheet.Cells[row, 3].Value = student.Name.LastName;
            }

            AssignmentResultEntry resultEntry = results.FirstOrDefault(entry => entry.UserId == enrolment.CanvasUserId);

            if (resultEntry is not null)
            {
                foreach (KeyValuePair<string, int> item in criteriaOrder)
                {
                    AssignmentResultEntry.AssignmentRubricResult result = resultEntry.Marks.FirstOrDefault(entry => entry.CriterionId == item.Key);

                    if (result is not null)
                    {
                        worksheet.Cells[row, item.Value].Value = result.Points;
                        
                        RubricEntry.RubricCriterionRating rating = rubric.Criteria.SelectMany(entry => entry.Ratings).FirstOrDefault(entry => entry.RatingId == result.RatingId);

                        worksheet.Cells[row, item.Value + 1].Value = rating.Name;
                    }
                }
            }

            worksheet.Cells[row, gradeColumn].Value = resultEntry.OverallPoints;
            worksheet.Cells[row, gradeColumn + 1].Value = resultEntry.OverallGrade;

            row++;
        }

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    public async Task<MemoryStream> CreateStudentAttendanceReport(
        string periodLabel,
        List<AttendanceRecord> records,
        List<AbsenceRecord> absenceRecords,
        CancellationToken cancellationToken = default)
    {
        ExcelPackage excel = new();

        foreach (Grade grade in Enum.GetValues<Grade>())
        {
            List<AttendanceRecord> filteredRecords = records
                .Where(entry => entry.Grade == grade)
                .ToList();

            if (filteredRecords.Count > 0)
            {
                BuildDataTableAndPivot(excel, periodLabel, grade, filteredRecords, absenceRecords);
            }
        }

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        excel.Dispose();
        return memoryStream;
    }

    private static void BuildDataTableAndPivot(ExcelPackage package, string periodLabel, Grade grade, List<AttendanceRecord> records, List<AbsenceRecord> absences)
    {
        ExcelWorksheet pivotWorksheet = package.Workbook.Worksheets.Add($"{grade.AsName()} Data");
        pivotWorksheet.Cells[1, 1].LoadFromCollection(records, true);

        pivotWorksheet.Cells[2, 9].Value = "90% - 100% Attendance";
        pivotWorksheet.Cells[2, 10].Formula = "=countif(E:E, I2)";

        pivotWorksheet.Cells[3, 9].Value = "75% - 90% Attendance";
        pivotWorksheet.Cells[3, 10].Formula = "=countif(E:E, I3)";

        pivotWorksheet.Cells[4, 9].Value = "50% - 75% Attendance";
        pivotWorksheet.Cells[4, 10].Formula = "=countif(E:E, I4)";

        pivotWorksheet.Cells[5, 9].Value = "Below 50% Attendance";
        pivotWorksheet.Cells[5, 10].Formula = "=countif(E:E, I5)";

        ExcelWorksheet chartWorksheet = package.Workbook.Worksheets.Add($"{grade.AsName()} Report");

        ExcelRangeBase chartTitle = chartWorksheet.Cells[1, 1, 1, 9];
        chartTitle.Merge = true;
        chartTitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        chartTitle.Style.Font.Name = "Maiandra GD";
        chartTitle.Style.Font.Size = 36;
        chartTitle.Style.Font.Color.SetColor(0, 91, 155, 213);
        chartTitle.Value = "Attendance Report";

        ExcelRangeBase chartSubtitle = chartWorksheet.Cells[2, 1, 2, 9];
        chartSubtitle.Merge = true;
        chartSubtitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        chartSubtitle.Style.Font.Name = "Maiandra GD";
        chartSubtitle.Style.Font.Size = 16;
        chartSubtitle.Value = $"YTD up to {periodLabel} - {grade.AsName()}";
        chartSubtitle.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
        chartSubtitle.Style.Border.Bottom.Color.SetColor(0, 0, 32, 96);

        // Create absence table
        ExcelRangeBase tableTitle = chartWorksheet.Cells[3, 6, 4, 9];
        tableTitle.Merge = true;
        tableTitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        tableTitle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        tableTitle.Style.Font.Name = "Maiandra GD";
        tableTitle.Style.Font.Size = 14;
        tableTitle.Value = "Students of Concern";

        chartWorksheet.Rows[5].Height = chartWorksheet.Row(5).Height * 2;
        chartWorksheet.Rows[5].Style.WrapText = true;

        ExcelRangeBase tableStudentColumn = chartWorksheet.Cells[5, 6];
        tableStudentColumn.Style.Font.Name = "Calibri";
        tableStudentColumn.Style.Font.Size = 10;
        tableStudentColumn.Style.Font.Bold = true;
        tableStudentColumn.Style.Fill.PatternType = ExcelFillStyle.Solid;
        tableStudentColumn.Style.Fill.BackgroundColor.SetColor(0, 155, 194, 230);
        tableStudentColumn.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
        tableStudentColumn.Value = "Student";
        chartWorksheet.Column(6).Width = chartWorksheet.Column(1).Width * 1.5;

        ExcelRangeBase tableReasonColumn = chartWorksheet.Cells[5, 7];
        tableReasonColumn.Style.Font.Name = "Calibri";
        tableReasonColumn.Style.Font.Size = 10;
        tableReasonColumn.Style.Font.Bold = true;
        tableReasonColumn.Style.Fill.PatternType = ExcelFillStyle.Solid;
        tableReasonColumn.Style.Fill.BackgroundColor.SetColor(0, 155, 194, 230);
        tableReasonColumn.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
        tableReasonColumn.Value = "Reason";
        chartWorksheet.Column(7).Width = chartWorksheet.Column(1).Width * 1.75;
        
        ExcelRangeBase tableDatesColumn = chartWorksheet.Cells[5, 8];
        tableDatesColumn.Style.Font.Name = "Calibri";
        tableDatesColumn.Style.Font.Size = 10;
        tableDatesColumn.Style.Font.Bold = true;
        tableDatesColumn.Style.Fill.PatternType = ExcelFillStyle.Solid;
        tableDatesColumn.Style.Fill.BackgroundColor.SetColor(0, 155, 194, 230);
        tableDatesColumn.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
        tableDatesColumn.Value = "Dates Absent";
        chartWorksheet.Column(8).Width = chartWorksheet.Column(1).Width * 1.5;

        ExcelRangeBase tableLessonsColumn = chartWorksheet.Cells[5, 9];
        tableLessonsColumn.Style.Font.Name = "Calibri";
        tableLessonsColumn.Style.Font.Size = 10;
        tableLessonsColumn.Style.Font.Bold = true;
        tableLessonsColumn.Style.Fill.PatternType = ExcelFillStyle.Solid;
        tableLessonsColumn.Style.Fill.BackgroundColor.SetColor(0, 155, 194, 230);
        tableLessonsColumn.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
        tableLessonsColumn.Value = "Lessons Missed";
        chartWorksheet.Column(9).Width = chartWorksheet.Column(1).Width * 1.5;

        List<StudentId> lowestStudentIds = records
            .Where(entry => entry.Group == "Below 50% Attendance")
            .Select(entry => entry.StudentId)
            .ToList();

        List<StudentId> lowerStudentIds = records
            .Where(entry => entry.Group == "50% - 75% Attendance")
            .Select(entry => entry.StudentId)
            .ToList();

        int rowNumber = 6;
        foreach (StudentId studentId in lowestStudentIds)
        {
            AttendanceRecord record = records.First(entry => entry.StudentId == studentId);

            chartWorksheet.Cells[rowNumber, 6].Value = record.StudentName.DisplayName;

            List<AbsenceRecord> absenceList = absences
                .Where(entry => entry.StudentId == studentId)
                .OrderBy(entry => entry.AbsenceDate)
                .ToList();

            if (absenceList.Count == 0)
            {
                chartWorksheet.Cells[rowNumber, 8].Value = "Nil";
                
                // Row border
                chartWorksheet.Cells[rowNumber, 6, rowNumber, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);

                // Cell border
                chartWorksheet.Cells[rowNumber, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
                chartWorksheet.Cells[rowNumber, 7].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
                chartWorksheet.Cells[rowNumber, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
                chartWorksheet.Cells[rowNumber, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);

                // Background fill
                chartWorksheet.Cells[rowNumber, 6, rowNumber, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                chartWorksheet.Cells[rowNumber, 6, rowNumber, 9].Style.Fill.BackgroundColor.SetColor(0, 255, 0, 0);

                // Row height
                chartWorksheet.Rows[rowNumber].Height = chartWorksheet.Row(2).Height * 2;
                chartWorksheet.Rows[rowNumber].Style.WrapText = true;

                chartWorksheet.Rows[rowNumber].Style.Font.Color.SetColor(Color.White);

                rowNumber++;

                continue;
            }

            int startRow = rowNumber;
            var groups = absenceList
                .OrderBy(absence => absence.AbsenceDate)
                .GroupBy(absence => new { absence.AbsenceReason, absence.AbsenceLesson });

            foreach (var group in groups)
            {
                string dateDisplay = BuildDateGroupLabel(group.Select(entry => entry.AbsenceDate).ToList());

                chartWorksheet.Cells[rowNumber, 7].Value = group.Key.AbsenceReason;
                chartWorksheet.Cells[rowNumber, 8].Value = dateDisplay;
                chartWorksheet.Cells[rowNumber, 9].Value = group.Key.AbsenceLesson;

                //Cell border
                chartWorksheet.Cells[rowNumber, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
                chartWorksheet.Cells[rowNumber, 7].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
                chartWorksheet.Cells[rowNumber, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
                chartWorksheet.Cells[rowNumber, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);

                // Background fill
                chartWorksheet.Cells[rowNumber, 6, rowNumber, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                chartWorksheet.Cells[rowNumber, 6, rowNumber, 9].Style.Fill.BackgroundColor.SetColor(0, 255, 0, 0);

                // Row height
                chartWorksheet.Rows[rowNumber].Height = chartWorksheet.Row(2).Height * 2;
                chartWorksheet.Rows[rowNumber].Style.WrapText = true;

                // Auto row height -- Upsets chart size, as the height of the row is set when the file is opened
                //chartWorksheet.Rows[rowNumber].CustomHeight = false;
                //chartWorksheet.Rows[rowNumber].Style.WrapText = true;

                rowNumber++;
            }

            // Row border
            chartWorksheet.Cells[startRow, 6, rowNumber - 1, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
            chartWorksheet.Cells[startRow, 6, rowNumber - 1, 9].Style.Font.Color.SetColor(Color.White);
        }

        foreach (StudentId studentId in lowerStudentIds)
        {
            AttendanceRecord record = records.First(entry => entry.StudentId == studentId);

            chartWorksheet.Cells[rowNumber, 6].Value = record.StudentName.DisplayName;

            List<AbsenceRecord> absenceList = absences
                .Where(entry => entry.StudentId == studentId)
                .ToList();

            if (absenceList.Count == 0)
            {
                chartWorksheet.Cells[rowNumber, 8].Value = "Nil";
                
                // Row border
                chartWorksheet.Cells[rowNumber, 6, rowNumber, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);

                // Cell border
                chartWorksheet.Cells[rowNumber, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
                chartWorksheet.Cells[rowNumber, 7].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
                chartWorksheet.Cells[rowNumber, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
                chartWorksheet.Cells[rowNumber, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);

                // Background fill
                chartWorksheet.Cells[rowNumber, 6, rowNumber, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                chartWorksheet.Cells[rowNumber, 6, rowNumber, 9].Style.Fill.BackgroundColor.SetColor(0, 255, 192, 0);

                // Row height
                chartWorksheet.Rows[rowNumber].Height = chartWorksheet.Row(2).Height * 2;
                chartWorksheet.Rows[rowNumber].Style.WrapText = true;

                rowNumber++;

                continue;
            }

            int startRow = rowNumber;
            var groups = absenceList
                .OrderBy(absence => absence.AbsenceDate)
                .GroupBy(absence => new { absence.AbsenceReason, absence.AbsenceLesson });

            foreach (var group in groups)
            {
                string dateDisplay = BuildDateGroupLabel(group.Select(entry => entry.AbsenceDate).ToList());

                chartWorksheet.Cells[rowNumber, 7].Value = group.Key.AbsenceReason;
                chartWorksheet.Cells[rowNumber, 8].Value = dateDisplay;
                chartWorksheet.Cells[rowNumber, 9].Value = group.Key.AbsenceLesson;

                // Cell border
                chartWorksheet.Cells[rowNumber, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
                chartWorksheet.Cells[rowNumber, 7].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
                chartWorksheet.Cells[rowNumber, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
                chartWorksheet.Cells[rowNumber, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);

                // Background fill
                chartWorksheet.Cells[rowNumber, 6, rowNumber, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                chartWorksheet.Cells[rowNumber, 6, rowNumber, 9].Style.Fill.BackgroundColor.SetColor(0, 255, 192, 0);

                // Row height
                chartWorksheet.Rows[rowNumber].Height = chartWorksheet.Row(2).Height * 2;
                chartWorksheet.Rows[rowNumber].Style.WrapText = true;

                // Auto row height -- Upsets chart size, as the height of the row is set when the file is opened
                //chartWorksheet.Rows[rowNumber].CustomHeight = false;
                //chartWorksheet.Rows[rowNumber].Style.WrapText = true;

                rowNumber++;
            }

            chartWorksheet.Cells[startRow, 6, rowNumber - 1, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.DarkBlue);
        }

        // Create Chart
        ExcelPieChart chart = chartWorksheet.Drawings.AddPieChart($"Chart{grade.AsNumber()}", ePieChartType.Pie);
        ExcelPieChartSerie series = chart.Series.Add(pivotWorksheet.Cells[2, 10, 5, 10], pivotWorksheet.Cells[2, 9, 5, 9]);
        ExcelChartDataPoint point0 = series.DataPoints.Add(0);
        point0.Fill.Style = eFillStyle.SolidFill;
        point0.Fill.SolidFill.Color.SetHslColor(97, 42, 79);

        ExcelChartDataPoint point1 = series.DataPoints.Add(1);
        point1.Fill.Style = eFillStyle.SolidFill;
        point1.Fill.SolidFill.Color.SetHslColor(45, 100, 90);

        ExcelChartDataPoint point2 = series.DataPoints.Add(2);
        point2.Fill.Style = eFillStyle.SolidFill;
        point2.Fill.SolidFill.Color.SetHslColor(45, 100, 50);

        ExcelChartDataPoint point3 = series.DataPoints.Add(3);
        point3.Fill.Style = eFillStyle.SolidFill;
        point3.Fill.SolidFill.Color.SetHslColor(0, 100, 50);

        chart.Title.Text = $"{grade.AsName()} Attendance Year to Date up to {periodLabel}";
        chart.Legend.Position = eLegendPosition.Bottom;
        chart.DataLabel.ShowPercent = true;
        chart.SetPosition(2, 5, 0, 5);
        chart.SetSize(300, 500);

        // Borders
        int bottom = chart.To.Row + 1 > rowNumber ? chart.To.Row + 1 : rowNumber;

        ExcelRange chartHorizontalDivider = chartWorksheet.Cells[bottom, 1, bottom, 9];
        chartHorizontalDivider.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
        chartHorizontalDivider.Style.Border.Bottom.Color.SetColor(0, 0, 32, 96);

        bottom++; 
        rowNumber = bottom;

        // Attendance Patterns
        ExcelRangeBase patternsTitle = chartWorksheet.Cells[rowNumber, 1, rowNumber, 5];
        patternsTitle.Merge = true;
        patternsTitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        patternsTitle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        patternsTitle.Style.Font.Name = "Maiandra GD";
        patternsTitle.Style.Font.Size = 14;
        patternsTitle.Value = "Attendance Patterns";

        rowNumber++;

        // 100% Attendance YTD
        ExcelRangeBase _100PercentTitle = chartWorksheet.Cells[rowNumber, 1, rowNumber, 5];
        _100PercentTitle.Merge = true;
        _100PercentTitle.Style.Font.Name = "Calibri";
        _100PercentTitle.Style.Font.Size = 11;
        _100PercentTitle.Style.Font.Bold = true;
        _100PercentTitle.Value = "100% Attendance YTD:";

        rowNumber++;

        IOrderedEnumerable<AttendanceRecord> perfectAttendanceRecords = records
            .Where(entry => entry.Percentage == 100)
            .OrderBy(entry => entry.StudentName.SortOrder);

        foreach (AttendanceRecord record in perfectAttendanceRecords)
        {
            ExcelRange recordRow = chartWorksheet.Cells[rowNumber, 1, rowNumber, 5];
            recordRow.Merge = true;
            recordRow.Value = record.StudentName.DisplayName;

            rowNumber++;
        }

        rowNumber++;

        // Attendance Improvements
        ExcelRangeBase improvementsTitle = chartWorksheet.Cells[rowNumber, 1, rowNumber, 5];
        improvementsTitle.Merge = true;
        improvementsTitle.Style.Font.Name = "Calibri";
        improvementsTitle.Style.Font.Size = 11;
        improvementsTitle.Style.Font.Bold = true;
        improvementsTitle.Value = "Improvement in attendance:";

        rowNumber++;

        IOrderedEnumerable<AttendanceRecord> improvementRecords = records
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Improvement))
            .OrderBy(entry => entry.StudentName.SortOrder);

        foreach (AttendanceRecord record in improvementRecords)
        {
            ExcelRange recordRow = chartWorksheet.Cells[rowNumber, 1, rowNumber, 5];
            recordRow.Merge = true;
            recordRow.Value = record.StudentName.DisplayName;
            ExcelComment comment = recordRow.AddComment(record.Improvement);
            comment.AutoFit = true;

            rowNumber++;
        }

        rowNumber++;

        // Decline in attendance
        ExcelRangeBase declineTitle = chartWorksheet.Cells[rowNumber, 1, rowNumber, 5];
        declineTitle.Merge = true;
        declineTitle.Style.Font.Name = "Calibri";
        declineTitle.Style.Font.Size = 11;
        declineTitle.Style.Font.Bold = true;
        declineTitle.Value = "Decline in attendance:";

        rowNumber++;

        IOrderedEnumerable<AttendanceRecord> declineRecords = records
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Decline))
            .OrderBy(entry => entry.StudentName.SortOrder);

        foreach (AttendanceRecord record in declineRecords)
        {
            ExcelRange recordRow = chartWorksheet.Cells[rowNumber, 1, rowNumber, 5];
            recordRow.Merge = true;
            recordRow.Value = record.StudentName.DisplayName;
            ExcelComment comment = recordRow.AddComment(record.Decline);
            comment.AutoFit = true;

            rowNumber++;
        }

        rowNumber++;

        (bottom, rowNumber) = (rowNumber, bottom); // Swap the bottom and rowNumber variables

        // Follow-up actions
        ExcelRangeBase followUpTitle = chartWorksheet.Cells[rowNumber, 6, rowNumber, 9];
        followUpTitle.Merge = true;
        followUpTitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        followUpTitle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        followUpTitle.Style.Font.Name = "Maiandra GD";
        followUpTitle.Style.Font.Size = 14;
        followUpTitle.Value = "Follow Up Actions";

        rowNumber += 2;

        ExcelRangeBase teacherTitle = chartWorksheet.Cells[rowNumber, 6, rowNumber, 9];
        teacherTitle.Merge = true;
        teacherTitle.Style.Font.Name = "Calibri";
        teacherTitle.Style.Font.Size = 11;
        teacherTitle.Style.Font.Bold = true;
        teacherTitle.Value = "Classroom Teacher Level:";

        rowNumber += 2;

        ExcelRangeBase assistantPrincipalTitle = chartWorksheet.Cells[rowNumber, 6, rowNumber, 9];
        assistantPrincipalTitle.Merge = true;
        assistantPrincipalTitle.Style.Font.Name = "Calibri";
        assistantPrincipalTitle.Style.Font.Size = 11;
        assistantPrincipalTitle.Style.Font.Bold = true;
        assistantPrincipalTitle.Value = "HT/AP Level:";

        rowNumber += 2;

        ExcelRangeBase htWellbeingTitle = chartWorksheet.Cells[rowNumber, 6, rowNumber, 9];
        htWellbeingTitle.Merge = true;
        htWellbeingTitle.Style.Font.Name = "Calibri";
        htWellbeingTitle.Style.Font.Size = 11;
        htWellbeingTitle.Style.Font.Bold = true;
        htWellbeingTitle.Value = "Head Teacher Wellbeing Level:";

        rowNumber += 2;

        ExcelRangeBase hpStudiesTitle = chartWorksheet.Cells[rowNumber, 6, rowNumber, 9];
        hpStudiesTitle.Merge = true;
        hpStudiesTitle.Style.Font.Name = "Calibri";
        hpStudiesTitle.Style.Font.Size = 11;
        hpStudiesTitle.Style.Font.Bold = true;
        hpStudiesTitle.Value = "Head Teacher Secondary Studies Level:";

        rowNumber += 2;

        ExcelRangeBase dpTitle = chartWorksheet.Cells[rowNumber, 6, rowNumber, 9];
        dpTitle.Merge = true;
        dpTitle.Style.Font.Name = "Calibri";
        dpTitle.Style.Font.Size = 11;
        dpTitle.Style.Font.Bold = true;
        dpTitle.Value = "Deputy Principal Level:";

        rowNumber += 2;
        
        // Vertical divider
        int lower = rowNumber > bottom ? rowNumber : bottom;

        ExcelRange chartVerticalDivider = chartWorksheet.Cells[3, 5, lower, 5];
        chartVerticalDivider.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        chartVerticalDivider.Style.Border.Right.Color.SetColor(0, 0, 32, 96);

        // Hide sheet
        pivotWorksheet.Hidden = eWorkSheetHidden.Hidden;
    }

    private static string BuildDateGroupLabel(List<DateOnly> group)
    {
        // Group the absence dates as well
        List<List<DateOnly>> dates = new();
        List<DateOnly> date = new() { group.First() };
        dates.Add(date);

        DateOnly lastDate = group.First();
        for (int i = 1; i < group.Count; i++)
        {
            DateOnly currentDate = group.ElementAt(i);
            int diff = currentDate.DayNumber - lastDate.DayNumber;
            if (diff > 1)
            {
                dates.Add(new List<DateOnly>());
            }

            dates.Last().Add(currentDate);
            lastDate = currentDate;
        }

        string dateDisplay = string.Empty;

        foreach (List<DateOnly> dateList in dates)
        {
            if (!string.IsNullOrEmpty(dateDisplay))
                dateDisplay += ", ";

            if (dateList.Count == 1)
            {
                dateDisplay += dateList[0].ToShortDateString();

                continue;
            }

            if (dateList.First().Month == dateList.Last().Month)
            {
                dateDisplay += dateList.First().Day;
                dateDisplay += "-";
                dateDisplay += dateList.Last().ToShortDateString();
            }
            else
            {
                dateDisplay += dateList.First().ToShortDateString();
                dateDisplay += "-";
                dateDisplay += dateList.Last().ToShortDateString();
            }
        }

        return dateDisplay;
    }

    private sealed record StudentRecord(
        string StudentId,
        string StudentName,
        string Grade,
        decimal StellarsEarned,
        decimal GalaxiesEarned,
        decimal UniversalsEarned);

    private class AwardRow
    {
        public string StudentId { get; init; }
        public string Surname { get; init; }
        public string FirstName { get; init; }
        public string RollClass { get; set; }
        public string Year { get; init; }
        public DateTime DateAwarded { get; set; }
        public string Category { get; set; }
        public string Award { get; set; }
        public int Level { get; set; }
        public int Value { get; init; }
        public int Total { get; init; }
    }

    private class IncidentRow
    {
        public int Age { get; set; }
        public DateOnly Date { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
        public string Teacher { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Year { get; set; }
    }
}
