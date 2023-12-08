namespace Constellation.Infrastructure.Services;

using Application.Attendance.GenerateAttendanceReportForPeriod;
using Application.Attendance.GetAttendanceDataFromSentral;
using Application.Compliance.GetWellbeingReportFromSentral;
using Application.Extensions;
using Application.Rollover.ImportStudents;
using Application.Training.Models;
using Constellation.Application.Absences.GetAbsencesWithFilterForReport;
using Constellation.Application.Awards.ExportAwardNominations;
using Constellation.Application.Contacts.GetContactList;
using Constellation.Application.DTOs;
using Constellation.Application.DTOs.CSV;
using Constellation.Application.ExternalDataConsistency;
using Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Infrastructure.Jobs;
using Core.Abstractions.Clock;
using Core.Extensions;
using ExcelDataReader;
using OfficeOpenXml;
using OfficeOpenXml.ConditionalFormatting.Contracts;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;

public class ExcelService : IExcelService
{
    private readonly IDateTimeProvider _dateTime;
    private static readonly Regex _csvParser = new(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

    public ExcelService(
        IDateTimeProvider dateTime)
    {
        _dateTime = dateTime;
    }

    public async Task<MemoryStream> CreatePTOFile(ICollection<InterviewExportDto> exportLines)
    {
        var excel = new ExcelPackage();
        var worksheet = excel.Workbook.Worksheets.Add("PTO");
        var headerRow = new List<string[]>()
        {
            new string[] { "SCD", "SFN", "SSN", "PCD1", "PTI1", "PFN1", "PSN1", "PEM1", "PCD2", "PTI2", "PFN2", "PSN2", "PEM2", "CCD", "CGN", "CLS", "TCD", "TTI", "TFN", "TSN", "TEM"}
        };

        //var headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

        worksheet.Cells[1, 1, 1, headerRow[0].Length].LoadFromArrays(headerRow);

        var rowData = new List<string[]>();

        foreach (var line in exportLines)
        {
            var row = new List<string>();

            row.Add(line.StudentId);
            row.Add(line.StudentFirstName);
            row.Add(line.StudentLastName);

            foreach (var parent in line.Parents)
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

        var stream = new MemoryStream();
        await excel.SaveAsAsync(stream);
        stream.Position = 0;

        return stream;
    }

    public async Task<MemoryStream> CreateAbsencesReportFile(
        List<FilteredAbsenceResponse> exportAbsences, 
        CancellationToken cancellationToken = default)
    {
        var excel = new ExcelPackage();
        var workSheet = excel.Workbook.Worksheets.Add("Sheet 1");
        //var pageTitle = workSheet.Cells[1, 1].RichText.Add();
        //pageTitle.Bold = true;
        //pageTitle.Size = 16;

        workSheet.Cells[2, 1].LoadFromCollection(exportAbsences, true);
        workSheet.Cells[2, 6, workSheet.Dimension.Rows, 6].Style.Numberformat.Format = "dd/MM/yyyy";
        workSheet.Cells[2, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        var memoryStream = new MemoryStream();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<MemoryStream> CreateAwardsCalculationFile(MemoryStream stream)
    {
        var awards = new List<AwardRow>();

        using (var p = new ExcelPackage(stream))
        {
            var ws = p.Workbook.Worksheets[0];
            var dataTable = ws.Cells[ws.Dimension.Address].ToDataTable();

            foreach (DataRow row in dataTable.Rows)
            {
                var date = DateTime.Parse(row["Date"].ToString());

                awards.Add(new AwardRow
                {
                    StudentId = row["Student Id"].ToString(),
                    Surname = row["Surname"].ToString(),
                    FirstName = row["First Name"].ToString(),
                    RollClass = row["Roll Class"].ToString(),
                    Year = row["Year"].ToString(),
                    DateAwarded = date,
                    Category = row["Category"].ToString(),
                    Award = row["Award"].ToString(),
                    Level = Convert.ToInt32(row["Level"].ToString()),
                    Value = Convert.ToInt32(row["Value"].ToString()),
                    Total = Convert.ToInt32(row["Total at Time"].ToString())
                });
            }
        }

        var exportStudents = new List<StudentRecord>();

        foreach (var student in awards.GroupBy(award => award.StudentId))
        {
            decimal stellarsEarned = 0;
            decimal galaxiesEarned = 0;
            decimal universalsEarned = 0;

            decimal totalIssued = student.Sum(award => award.Value);
            decimal weekStart = student.Max(award => award.Total) - totalIssued;

            stellarsEarned = Math.Floor(totalIssued / 5);
            stellarsEarned += Math.Floor(((weekStart % 5) + (totalIssued % 5)) / 5);

            galaxiesEarned = Math.Floor(totalIssued / 25);
            galaxiesEarned += Math.Floor(((weekStart % 25) + (totalIssued % 25)) / 25);

            universalsEarned = Math.Floor(totalIssued / 125);
            universalsEarned += Math.Floor(((weekStart % 125) + (totalIssued % 125)) / 125);

            if (stellarsEarned > 0)
            {
                exportStudents.Add(new StudentRecord
                {
                    StudentId = student.First().StudentId,
                    StudentName = $"{student.First().FirstName} {student.First().Surname}",
                    Grade = student.First().Year,
                    StellarsEarned = stellarsEarned,
                    GalaxiesEarned = galaxiesEarned,
                    UniveralsEarned = universalsEarned
                });
            }
        }

        var excel = new ExcelPackage();
        var worksheet = excel.Workbook.Worksheets.Add("Awards");
        var headerRow = new List<string[]>()
        {
            new string[] { "Student Id", "Student Name", "Grade", "Stellars", "Galaxies", "Universal Achievers" }
        };

        var headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

        worksheet.Cells[headerRange].LoadFromArrays(headerRow);

        if (exportStudents.Count > 0)
        {
            worksheet.Cells[2, 1].LoadFromCollection(exportStudents);
        }

        var resultStream = new MemoryStream();
        await excel.SaveAsAsync(resultStream);
        resultStream.Position = 0;

        return resultStream;
    }

    public async Task<MemoryStream> CreateTrainingModuleReportFile(ModuleDetailsDto data)
    {
        Type completion = typeof(CompletionRecordDto);

        ExcelPackage excel = new();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Sheet 1");

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
                completion.GetProperty("StaffId"),
                completion.GetProperty("StaffFirstName"),
                completion.GetProperty("StaffLastName"),
                completion.GetProperty("StaffFaculty"),
                completion.GetProperty("NotMandatory"),
                completion.GetProperty("ExpiryCountdown"),
                completion.GetProperty("CompletedDate")
            };
        });

        workSheet.Cells[7, 7, workSheet.Dimension.Rows, 7].Style.Numberformat.Format = "dd/MM/yyyy";

        // Highlight overdue entries
        ExcelAddress dataRange = new ExcelAddress(8, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns);

        IExcelConditionalFormattingExpression formatNotRequired = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatNotRequired.Formula = "=$E8 = TRUE";
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

        MemoryStream memoryStream = new MemoryStream();
        await excel.SaveAsAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<MemoryStream> CreateTrainingModuleStaffReportFile(StaffCompletionListDto data)
    {
        Type completion = typeof(CompletionRecordExtendedDetailsDto);

        ExcelPackage excel = new ExcelPackage();
        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Sheet 1");

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
                completion.GetProperty("ModuleName"),
                completion.GetProperty("ModuleFrequency"),
                completion.GetProperty("RequiredByRoles"),
                completion.GetProperty("TimeToExpiry"),
                completion.GetProperty("RecordEffectiveDate"),
                completion.GetProperty("DueDate")
            };
        });

        workSheet.Cells[7, 5, workSheet.Dimension.Rows, 6].Style.Numberformat.Format = "dd/MM/yyyy";

        // Highlight overdue entries
        ExcelAddress dataRange = new ExcelAddress(8, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns);

        IExcelConditionalFormattingExpression formatNeverCompleted = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatNeverCompleted.Formula = "=$D8 = -9999";
        formatNeverCompleted.Style.Fill.BackgroundColor.Color = Color.Gray;
        formatNeverCompleted.Style.Font.Color.Color = Color.White;
        formatNeverCompleted.StopIfTrue = true;

        IExcelConditionalFormattingExpression formatOverdue = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatOverdue.Formula = "=$D8 < 1";
        formatOverdue.Style.Fill.BackgroundColor.Color = Color.Red;
        formatOverdue.Style.Font.Color.Color = Color.White;
        formatOverdue.StopIfTrue = true;

        IExcelConditionalFormattingExpression formatSoonExpire = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatSoonExpire.Formula = "=$D8 < 14";
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

        MemoryStream memoryStream = new MemoryStream();
        await excel.SaveAsAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public List<TrainingModule> ImportMandatoryTrainingDataFromFile(MemoryStream excelFile)
    {
        ExcelPackage excel = new ExcelPackage(excelFile);
        ExcelWorksheet workSheet = excel.Workbook.Worksheets[0];

        int numModules = workSheet.Dimension.Rows;

        List<TrainingModule> modules = new List<TrainingModule>();

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

        return modules;
    }

    public async Task<MemoryStream> CreateGroupTutorialAttendanceFile(TutorialDetailsDto data)
    {
        var excel = new ExcelPackage();
        var workSheet = excel.Workbook.Worksheets.Add("Sheet 1");

        var nameDetail = workSheet.Cells[1, 1].RichText.Add(data.Name);
        nameDetail.Bold = true;
        nameDetail.Size = 16;

        var dateDetail = workSheet.Cells[2, 1].RichText.Add($"From {data.StartDate.ToShortDateString()} to {data.EndDate.ToShortDateString()}");
        dateDetail.Bold = true;
        dateDetail.Size = 16;

        var students = data.Rolls
            .SelectMany(roll => roll.Students)
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.Name)
            .GroupBy(student => student.StudentId)
            .ToList();

        var rolls = data.Rolls.OrderBy(roll => roll.SessionDate).ToList();

        int startColumn = 3;
        int startRow = 5;

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
            var student = students[i];

            workSheet.Cells[startRow + i, startColumn].Value = student.First().Name;
            workSheet.Cells[startRow + i, startColumn + 1].Value = student.First().Grade;

            for(int j = 0; j < rolls.Count; j++)
            {
                string text = string.Empty;
                bool enrolled = false;

                var entry = rolls[j].Students.FirstOrDefault(student => student.StudentId == students[i].Key);

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

                if (enrolled)
                {
                    workSheet.Cells[startRow + i, startColumn + 2 + j].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    workSheet.Cells[startRow + i, startColumn + 2 + j].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                } 
                else
                {
                    workSheet.Cells[startRow + i, startColumn + 2 + j].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    workSheet.Cells[startRow + i, startColumn + 2 + j].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }
            }
        }

        workSheet.View.FreezePanes(7, 3);
        workSheet.Cells[5, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        var memoryStream = new MemoryStream();
        await excel.SaveAsAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<MemoryStream> CreateFamilyContactDetailsChangeReport(List<ParentContactChangeDto> changes, CancellationToken cancellationToken = default)
    {
        var excel = new ExcelPackage();
        var workSheet = excel.Workbook.Worksheets.Add("Sheet 1");

        var nameDetail = workSheet.Cells[1, 1].RichText.Add("Parent Contact Details Changed");
        nameDetail.Bold = true;
        nameDetail.Size = 16;

        var dateDetail = workSheet.Cells[2, 1].RichText.Add($"Report generated on {DateTime.Today.ToLongDateString()}");
        dateDetail.Bold = true;
        dateDetail.Size = 16;

        workSheet.Cells[4, 1].LoadFromCollection(changes, true);

        workSheet.View.FreezePanes(5, 1);
        workSheet.Cells[4, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFilter = true;
        workSheet.Cells[5, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        var memoryStream = new MemoryStream();
        await excel.SaveAsAsync(memoryStream, cancellationToken);
        
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<List<MasterFileSchool>> GetSchoolsFromMasterFile(MemoryStream stream)
    {
        List<MasterFileSchool> schoolsList = new();

        using ExcelPackage package = new(stream);
        ExcelWorksheet worksheet = package.Workbook.Worksheets.First(sheet => sheet.Name == "Partner_schools");
        int rows = worksheet.Dimension.Rows;

        for (int i = 2; i <= rows; i++)
        {
            var codeCell = worksheet.Cells[i, 9].Value;
            if (codeCell is null)
                continue;

            var code = codeCell.ToString().Trim();

            var nameCell = worksheet.Cells[i, 1].Value;
            if (nameCell is null)
                continue;

            var name = ((string)nameCell).Trim();

            var statusCell = worksheet.Cells[i, 2].Value;
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

            var principalCell = worksheet.Cells[i, 18].Value;
            var principal = (principalCell is not null) ? ((string)principalCell).Trim() : string.Empty;

            var principalEmailCell = worksheet.Cells[i, 20].Value;
            var principalEmail = (principalEmailCell is not null) ? ((string)principalEmailCell).Trim() : string.Empty;

            schoolsList.Add(new MasterFileSchool(
                i,
                code,
                name,
                siteStatus,
                principal,
                principalEmail));
        }

        return schoolsList;
    }

    public async Task<List<MasterFileStudent>> GetStudentsFromMasterFile(MemoryStream stream)
    {
        List<MasterFileStudent> studentList = new();

        using ExcelPackage package = new(stream);
        ExcelWorksheet worksheet = package.Workbook.Worksheets.First(sheet => sheet.Name == "Students_2023");
        int rows = worksheet.Dimension.Rows;

        for (int i = 2; i <= rows; i++)
        {
            var srnCell = worksheet.Cells[i, 1].Value;
            if (srnCell is null)
                continue;

            var srn = srnCell.ToString().Trim();

            var fNameCell = worksheet.Cells[i, 3].Value;
            if (fNameCell is null)
                continue;

            var fName = ((string)fNameCell).Trim();

            var sNameCell = worksheet.Cells[i, 4].Value;
            if (sNameCell is null)
                continue;

            var sName = ((string)sNameCell).Trim();

            var gradeCell = worksheet.Cells[i, 6].Value;
            if (gradeCell is null)
                continue;

            Grade grade = gradeCell.ToString().Trim() switch
            {
                "5" => Grade.Y05,
                "6" => Grade.Y06,
                "6*" => Grade.Y06,
                "7" => Grade.Y07,
                "8" => Grade.Y08,
                "9" => Grade.Y09,
                "10" => Grade.Y10,
                "11" => Grade.Y11,
                "12" => Grade.Y12,
                _ => Grade.SpecialProgram
            };

            var parent1Cell = worksheet.Cells[i, 42].Value as string;
            string parent1 = string.Empty;
            if (parent1Cell is not null && !string.IsNullOrWhiteSpace(parent1Cell))
                parent1 = parent1Cell.Trim();

            var parent2Cell = worksheet.Cells[i, 43].Value as string;
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

        return studentList;
    }

    public async Task<MemoryStream> CreateMasterFileConsistencyReport(List<UpdateItem> updateItems, CancellationToken cancellationToken = default)
    {
        var excel = new ExcelPackage();
        var workSheet = excel.Workbook.Worksheets.Add("Sheet 1");

        var nameDetail = workSheet.Cells[1, 1].RichText.Add("MasterFile Consistency Report");
        nameDetail.Bold = true;
        nameDetail.Size = 16;

        workSheet.Cells[3, 1].LoadFromCollection(updateItems, true);

        workSheet.View.FreezePanes(4, 1);
        workSheet.Cells[3, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFilter = true;
        workSheet.Cells[4, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        var memoryStream = new MemoryStream();
        await excel.SaveAsAsync(memoryStream, cancellationToken);

        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<MemoryStream> CreateContactExportFile(List<ContactResponse> contacts, CancellationToken cancellationToken = default)
    {
        var excel = new ExcelPackage();
        var workSheet = excel.Workbook.Worksheets.Add("Contacts");

        workSheet.Cells[1, 1].LoadFromCollection(contacts, true);

        workSheet.View.FreezePanes(2, 1);
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFilter = true;
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        var memoryStream = new MemoryStream();
        await excel.SaveAsAsync(memoryStream, cancellationToken);

        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<MemoryStream> CreateAwardNominationsExportFile(List<AwardNominationExportDto> nominations, CancellationToken cancellationToken = default)
    {
        var excel = new ExcelPackage();
        var workSheet = excel.Workbook.Worksheets.Add("Nominations");

        workSheet.Cells[1, 1].LoadFromCollection(nominations, true);

        workSheet.View.FreezePanes(2, 1);
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFilter = true;
        workSheet.Cells[1, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        var memoryStream = new MemoryStream();
        await excel.SaveAsAsync(memoryStream, cancellationToken);

        memoryStream.Position = 0;

        return memoryStream;
    }


    public List<StudentAttendanceData> ExtractPerDayYearToDateAttendanceData(SystemAttendanceData systemData, List<StudentAttendanceData> data)
    {
        if (systemData.YearToDateDayCalculationDocument is null)
            return data;

        List<string> ytdDayData = systemData.YearToDateDayCalculationDocument.DocumentNode.InnerHtml.Split('\u000A').ToList();

        // Remove first and last entry
        ytdDayData.RemoveAt(0);
        ytdDayData.RemoveAt(ytdDayData.Count - 1);

        foreach (string row in ytdDayData)
        {
            string[] line = _csvParser.Split(row);

            // Index 0: Surname
            // Index 1: Preferred name
            // Index 2: Rollclass name
            // Index 3: School year
            // Index 4: External id
            // Index 5: Days total
            // Index 6: Days absent
            // Index 7: Days attended
            // Index 8: Percentage attendance
            // Index 9: Percentage absent
            // Index 10: Explained absences
            // Index 11: Unexplained absences
            // Index 12: Percentage explained
            // Index 13: Percentage unexplained

            string studentId = line[4].FormatField();

            StudentAttendanceData entry = data.FirstOrDefault(entry => entry.StudentId == studentId);

            if (entry is not null)
            {
                entry.DayYTD = Convert.ToDecimal(line[8].FormatField());
            }
            else
            {
                entry = new()
                {
                    StudentId = studentId,
                    Name = $"{line[1].FormatField()} {line[0].FormatField()}",
                    Grade = (Grade)Convert.ToInt32(line[3].FormatField()),
                    DayYTD = Convert.ToDecimal(line[8].FormatField())
                };

                data.Add(entry);
            }
        }

        return data;
    }

    public List<StudentAttendanceData> ExtractPerMinuteYearToDateAttendanceData(SystemAttendanceData systemData, List<StudentAttendanceData> data)
    {
        if (systemData.YearToDateMinuteCalculationDocument is null)
            return data;

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
            // Index 9: Untallied Time
            // Index 10: Percentage

            string studentId = row[0].ToString();

            StudentAttendanceData entry = data.FirstOrDefault(entry => entry.StudentId == studentId);

            if (entry is not null)
            {
                entry.MinuteYTD = Convert.ToDecimal(row[10]);
            }
            else
            {
                entry = new()
                {
                    StudentId = studentId,
                    Name = $"{row[1].ToString().FormatField()} {row[2].ToString().FormatField()}",
                    MinuteYTD = Convert.ToDecimal(row[10])
                };

                data.Add(entry);
            }
        }

        return data;
    }

    public List<StudentAttendanceData> ExtractPerDayWeekAttendanceData(SystemAttendanceData systemData, List<StudentAttendanceData> data)
    {
        if (systemData.WeekDayCalculationDocument is null)
            return data;

        List<string> fnDayData = systemData.WeekDayCalculationDocument.DocumentNode.InnerHtml.Split('\u000A').ToList();

        // Remove first and last entry
        fnDayData.RemoveAt(0);
        fnDayData.RemoveAt(fnDayData.Count - 1);

        foreach (string row in fnDayData)
        {
            string[] line = _csvParser.Split(row);

            // Index 0: Surname
            // Index 1: Preferred name
            // Index 2: Rollclass name
            // Index 3: School year
            // Index 4: External id
            // Index 5: Days total
            // Index 6: Days absent
            // Index 7: Days attended
            // Index 8: Percentage attendance
            // Index 9: Percentage absent
            // Index 10: Explained absences
            // Index 11: Unexplained absences
            // Index 12: Percentage explained
            // Index 13: Percentage unexplained

            string studentId = line[4].FormatField();

            StudentAttendanceData entry = data.FirstOrDefault(entry => entry.StudentId == studentId);

            if (entry is not null)
            {
                entry.DayWeek = Convert.ToDecimal(line[8].FormatField());
            }
            else
            {
                entry = new()
                {
                    StudentId = studentId,
                    Name = $"{line[1].FormatField()} {line[0].FormatField()}",
                    DayWeek = Convert.ToDecimal(line[8].FormatField())
                };

                data.Add(entry);
            }
        }

        return data;
    }

    public List<StudentAttendanceData> ExtractPerMinuteWeekAttendanceData(SystemAttendanceData systemData, List<StudentAttendanceData> data)
    {
        if (systemData.WeekMinuteCalculationDocument is null)
            return data;

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
            // Index 9: Untallied Time
            // Index 10: Percentage

            string studentId = row[0].ToString();

            StudentAttendanceData entry = data.FirstOrDefault(entry => entry.StudentId == studentId);

            if (entry is not null)
            {
                entry.MinuteWeek = Convert.ToDecimal(row[10]);
            }
            else
            {
                entry = new()
                {
                    StudentId = studentId,
                    Name = $"{row[1].ToString().FormatField()} {row[2].ToString().FormatField()}",
                    MinuteWeek = Convert.ToDecimal(row[10])
                };

                data.Add(entry);
            }
        }

        return data;
    }

    public async Task<List<SentralIncidentDetails>> ConvertSentralIncidentReport(Stream reportFile, CancellationToken cancellationToken = default)
    {
        List<SentralIncidentDetails> response = new();

        if (reportFile is null)
            return response;

        using IExcelDataReader reader = ExcelReaderFactory.CreateReader(reportFile);
        DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration { UseColumnDataType = true });

        foreach (DataRow row in result.Tables[0].Rows)
        {
            if (row.ItemArray.First()?.ToString() == "Student Id") // This is a header row
                continue;

            // Index 0: Student Id
            // Index 1: Date Created
            // Index 2: Date of Incident
            // Index 3: Day
            // Index 4: Incident
            // Index 5: Student Was
            // Index 6: Incident Time
            // Index 7: Period
            // Index 8: Subject
            // Index 9: Category
            // Index 10: Type
            // Index 11: Sub Type
            // Index 12: Incident Records Description
            // Index 13: Incident Record Details
            // Index 14: Incident Record Detail Options
            // Index 15: Follow Up Action Comment
            // Index 16: Follow Up Actions
            // Index 17: Teacher
            // Index 18: Student Surname,
            // Index 19: Student First Name,
            // Index 20: DOB
            // Index 21: Years
            // Index 22: Months
            // Index 23: School Year
            // Index 24: House
            // Index 25: Roll Class
            // Index 26: Location

            string studentId = row[0].ToString();

            DateOnly.TryParse(row[1].ToString(), out DateOnly dateCreated);

            int severity = _dateTime.Today.DayNumber - dateCreated.DayNumber;

            int gradeNum = Convert.ToInt32(row[23]);
            Grade grade = (Grade)gradeNum;
            
            response.Add(new(
                studentId,
                dateCreated,
                row[4].ToString().FormatField(),
                row[8].ToString().FormatField(),
                row[10].ToString().FormatField(),
                row[17].ToString().FormatField(),
                row[19].ToString().FormatField(),
                row[18].ToString().FormatField(),
                grade,
                severity));
        }

        return response;
    }

    public async Task<List<StudentImportRecord>> ConvertStudentImportFile(MemoryStream importFile, CancellationToken cancellationToken = default)
    {
        List<StudentImportRecord> results = new();

        using IExcelDataReader reader = ExcelReaderFactory.CreateReader(importFile);

        DataSet sheet = reader.AsDataSet();
        DataRowCollection rows = sheet.Tables[0].Rows;
        foreach (DataRow row in rows)
        {
            if (row[0].ToString() == "StudentId")
                continue;

            string studentId = row[0]?.ToString() ?? string.Empty;
            string firstName = row[1]?.ToString() ?? string.Empty;
            string lastName = row[2]?.ToString() ?? string.Empty;
            string userName = row[3]?.ToString() ?? string.Empty;
            string grade = row[4]?.ToString() ?? string.Empty;
            string schoolCode = row[5]?.ToString() ?? string.Empty;
            string gender = row[6]?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(studentId) ||
                string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(grade) ||
                string.IsNullOrWhiteSpace(schoolCode) ||
                string.IsNullOrWhiteSpace(gender))
            {
                continue;
            }

            bool success = Enum.TryParse(grade, true, out Grade gradeVal);

            if (!success)
            {
                continue;
            }

            results.Add(new(
                studentId,
                firstName,
                lastName,
                userName,
                gradeVal,
                schoolCode,
                gender));
        }

        return results;
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
            .ToList();

        ExcelPackage excel = new();

        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Sheet 1");

        ExcelRangeBase table = worksheet.Cells[1, 1].LoadFromCollection(rows, true);
        worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns].Style.Font.Bold = true;

        ExcelRangeBase data = worksheet.Cells[2, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns];

        worksheet.View.FreezePanes(2, 1);

        table.AutoFilter = true;
        table.AutoFitColumns();

        IExcelConditionalFormattingExpression bandFourFormat = worksheet.ConditionalFormatting.AddExpression(data);
        bandFourFormat.Formula = "=$A2 > 34";
        bandFourFormat.Style.Fill.PatternType = ExcelFillStyle.Solid;
        bandFourFormat.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 196, 89, 17));
        bandFourFormat.Style.Font.Color.SetColor(Color.White);
        bandFourFormat.StopIfTrue = true;

        IExcelConditionalFormattingExpression bandThreeFormat = worksheet.ConditionalFormatting.AddExpression(data);
        bandThreeFormat.Formula = "=$A2 > 27";
        bandThreeFormat.Style.Fill.PatternType = ExcelFillStyle.Solid;
        bandThreeFormat.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 244, 176, 131));
        bandThreeFormat.StopIfTrue = true;

        IExcelConditionalFormattingExpression bandTwoFormat = worksheet.ConditionalFormatting.AddExpression(data);
        bandTwoFormat.Formula = "=$A2 > 20";
        bandTwoFormat.Style.Fill.PatternType = ExcelFillStyle.Solid;
        bandTwoFormat.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 241, 202, 172));
        bandTwoFormat.StopIfTrue = true;

        IExcelConditionalFormattingExpression bandOneFormat = worksheet.ConditionalFormatting.AddExpression(data);
        bandOneFormat.Formula = "=$A2 > 13";
        bandOneFormat.Style.Fill.PatternType = ExcelFillStyle.Solid;
        bandOneFormat.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 251, 228, 213));
        bandOneFormat.StopIfTrue = true;

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);

        memoryStream.Position = 0;

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

            if (filteredRecords.Any())
            {
                BuildDataTableAndPivot(excel, periodLabel, grade, filteredRecords, absenceRecords);
            }
        }

        MemoryStream memoryStream = new();
        await excel.SaveAsAsync(memoryStream, cancellationToken);

        memoryStream.Position = 0;

        return memoryStream;
    }

    private void BuildDataTableAndPivot(ExcelPackage package, string periodLabel, Grade grade, List<AttendanceRecord> records, List<AbsenceRecord> absences)
    {
        ExcelWorksheet pivotWorksheet = package.Workbook.Worksheets.Add($"{grade.AsName()} Data");
        ExcelRangeBase table = pivotWorksheet.Cells[1, 1].LoadFromCollection(records, true);

        pivotWorksheet.Cells[2, 9].Value = "90% - 100% Attendance";
        pivotWorksheet.Cells[2, 10].Formula = "=countif(E:E, I2)";

        pivotWorksheet.Cells[3, 9].Value = "75% - 90% Attendance";
        pivotWorksheet.Cells[3, 10].Formula = "=countif(E:E, I3)";

        pivotWorksheet.Cells[4, 9].Value = "50% - 75% Attendance";
        pivotWorksheet.Cells[4, 10].Formula = "=countif(E:E, I4)";

        pivotWorksheet.Cells[5, 9].Value = "Below 50% Attendance";
        pivotWorksheet.Cells[5, 10].Formula = "=countif(E:E, I5)";

        ExcelRangeBase chartRange = pivotWorksheet.Cells[2, 9, 5, 10];
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

        List<string> lowestStudentIds = records
            .Where(entry => entry.Group == "Below 50% Attendance")
            .Select(entry => entry.StudentId)
            .ToList();

        List<string> lowerStudentIds = records
            .Where(entry => entry.Group == "50% - 75% Attendance")
            .Select(entry => entry.StudentId)
            .ToList();

        int rowNumber = 6;
        foreach (string studentId in lowestStudentIds)
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
                string dateDisplay = BuildDateGroupLabel(group.Select(entry => entry.AbsenceDate));

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

        foreach (string studentId in lowerStudentIds)
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
                string dateDisplay = BuildDateGroupLabel(group.Select(entry => entry.AbsenceDate));

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

        // Follow up actions
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

    private string BuildDateGroupLabel(IEnumerable<DateOnly> group)
    {
        // Group the absence dates as well
        List<List<DateOnly>> dates = new();
        List<DateOnly> date = new() { group.First() };
        dates.Add(date);

        DateOnly lastDate = group.First();
        for (int i = 1; i < group.Count(); i++)
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
            if (dateDisplay != string.Empty)
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

    private class StudentRecord
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string Grade { get; set; }
        public decimal StellarsEarned { get; set; }
        public decimal GalaxiesEarned { get; set; }
        public decimal UniveralsEarned { get; set; }
    }

    private class AwardRow
    {
        public string StudentId { get; set; }
        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string RollClass { get; set; }
        public string Year { get; set; }
        public DateTime DateAwarded { get; set; }
        public string Category { get; set; }
        public string Award { get; set; }
        public int Level { get; set; }
        public int Value { get; set; }
        public int Total { get; set; }
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
