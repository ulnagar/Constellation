namespace Constellation.Infrastructure.Services;

using Constellation.Application.Absences.GetAbsencesForExport;
using Constellation.Application.Absences.GetAbsencesWithFilterForReport;
using Constellation.Application.Contacts.GetContactList;
using Constellation.Application.DTOs;
using Constellation.Application.DTOs.CSV;
using Constellation.Application.ExternalDataConsistency;
using Constellation.Application.GroupTutorials.GenerateTutorialAttendanceReport;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Jobs;
using OfficeOpenXml;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Threading.Channels;

public class ExcelService : IExcelService
{
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
        var completion = typeof(CompletionRecordDto);

        var excel = new ExcelPackage();
        var workSheet = excel.Workbook.Worksheets.Add("Sheet 1");

        var nameDetail = workSheet.Cells[1, 1].RichText.Add(data.Name);
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
                completion.GetProperty("NotRequired"),
                completion.GetProperty("ExpiryCountdown"),
                completion.GetProperty("CompletedDate")
            };
        });

        workSheet.Cells[7, 7, workSheet.Dimension.Rows, 7].Style.Numberformat.Format = "dd/MM/yyyy";

        // Highlight overdue entries
        var dataRange = new ExcelAddress(8, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns);

        var formatNotRequired = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatNotRequired.Formula = "=$E8 = TRUE";
        formatNotRequired.Style.Font.Color.Color = Color.DarkOliveGreen;
        formatNotRequired.Style.Font.Italic = true;
        formatNotRequired.StopIfTrue = true;

        var formatNeverCompleted = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatNeverCompleted.Formula = "=$F8 = -9999";
        formatNeverCompleted.Style.Fill.BackgroundColor.Color = Color.Gray;
        formatNeverCompleted.Style.Font.Color.Color = Color.White;
        formatNeverCompleted.StopIfTrue = true;

        var formatOverdue = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatOverdue.Formula = "=$F8 < 1";
        formatOverdue.Style.Fill.BackgroundColor.Color = Color.Red;
        formatOverdue.Style.Font.Color.Color = Color.White;
        formatOverdue.StopIfTrue = true;

        var formatSoonExpire = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatSoonExpire.Formula = "=$F8 < 14";
        formatSoonExpire.Style.Fill.BackgroundColor.Color = Color.Yellow;
        formatSoonExpire.StopIfTrue = true;

        // Add format legend
        workSheet.Cells[5, 2].Value = "Colour Legend";
        workSheet.Cells[5, 2].Style.Font.Bold = true;
        workSheet.Cells[5, 3].Value = "Expired";
        workSheet.Cells[5, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        workSheet.Cells[5, 3].Style.Fill.BackgroundColor.SetColor(Color.Red);
        workSheet.Cells[5, 3].Style.Font.Color.SetColor(Color.White);
        workSheet.Cells[5, 4].Value = "Expiring Soon";
        workSheet.Cells[5, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        workSheet.Cells[5, 4].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
        workSheet.Cells[5, 5].Value = "Never Completed";
        workSheet.Cells[5, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        workSheet.Cells[5, 5].Style.Fill.BackgroundColor.SetColor(Color.Gray);
        workSheet.Cells[5, 5].Style.Font.Color.SetColor(Color.White);
        workSheet.Cells[5, 6].Value = "Not Required";
        workSheet.Cells[5, 6].Style.Font.Color.SetColor(Color.DarkOliveGreen);
        workSheet.Cells[5, 6].Style.Font.Italic = true;
        workSheet.Cells[5, 2, 5, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
        workSheet.Cells[5, 3, 5, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        // Freeze top rows
        workSheet.View.FreezePanes(8, 1);
        workSheet.Cells[5, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        var memoryStream = new MemoryStream();
        await excel.SaveAsAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<MemoryStream> CreateTrainingModuleStaffReportFile(StaffCompletionListDto data)
    {
        var completion = typeof(CompletionRecordExtendedDetailsDto);

        var excel = new ExcelPackage();
        var workSheet = excel.Workbook.Worksheets.Add("Sheet 1");

        var nameDetail = workSheet.Cells[1, 1].RichText.Add(data.Name);
        nameDetail.Bold = true;
        nameDetail.Size = 16;

        var facultyText = "Faculties: ";
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
                completion.GetProperty("RecordNotRequired"),
                completion.GetProperty("TimeToExpiry"),
                completion.GetProperty("RecordEffectiveDate"),
                completion.GetProperty("DueDate")
            };
        });

        workSheet.Cells[7, 5, workSheet.Dimension.Rows, 6].Style.Numberformat.Format = "dd/MM/yyyy";

        // Highlight overdue entries
        var dataRange = new ExcelAddress(8, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns);

        var formatNotRequired = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatNotRequired.Formula = "=$C8 = TRUE";
        formatNotRequired.Style.Font.Color.Color = Color.DarkOliveGreen;
        formatNotRequired.Style.Font.Italic = true;
        formatNotRequired.StopIfTrue = true;

        var formatNeverCompleted = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatNeverCompleted.Formula = "=$D8 = -9999";
        formatNeverCompleted.Style.Fill.BackgroundColor.Color = Color.Gray;
        formatNeverCompleted.Style.Font.Color.Color = Color.White;
        formatNeverCompleted.StopIfTrue = true;

        var formatOverdue = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatOverdue.Formula = "=$D8 < 1";
        formatOverdue.Style.Fill.BackgroundColor.Color = Color.Red;
        formatOverdue.Style.Font.Color.Color = Color.White;
        formatOverdue.StopIfTrue = true;

        var formatSoonExpire = workSheet.ConditionalFormatting.AddExpression(dataRange);
        formatSoonExpire.Formula = "=$D8 < 14";
        formatSoonExpire.Style.Fill.BackgroundColor.Color = Color.Yellow;
        formatSoonExpire.StopIfTrue = true;

        // Add format legend
        workSheet.Cells[5, 2].Value = "Colour Legend";
        workSheet.Cells[5, 2].Style.Font.Bold = true;
        workSheet.Cells[5, 3].Value = "Expired";
        workSheet.Cells[5, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        workSheet.Cells[5, 3].Style.Fill.BackgroundColor.SetColor(Color.Red);
        workSheet.Cells[5, 3].Style.Font.Color.SetColor(Color.White);
        workSheet.Cells[5, 4].Value = "Expiring Soon";
        workSheet.Cells[5, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        workSheet.Cells[5, 4].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
        workSheet.Cells[5, 5].Value = "Never Completed";
        workSheet.Cells[5, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        workSheet.Cells[5, 5].Style.Fill.BackgroundColor.SetColor(Color.Gray);
        workSheet.Cells[5, 5].Style.Font.Color.SetColor(Color.White);
        workSheet.Cells[5, 6].Value = "Not Required";
        workSheet.Cells[5, 6].Style.Font.Color.SetColor(Color.DarkOliveGreen);
        workSheet.Cells[5, 6].Style.Font.Italic = true;
        workSheet.Cells[5, 2, 5, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
        workSheet.Cells[5, 3, 5, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        // Freeze top rows
        workSheet.View.FreezePanes(8, 1);
        workSheet.Cells[5, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

        var memoryStream = new MemoryStream();
        await excel.SaveAsAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public List<TrainingModule> ImportMandatoryTrainingDataFromFile(MemoryStream excelFile)
    {
        var excel = new ExcelPackage(excelFile);
        var workSheet = excel.Workbook.Worksheets[0];

        var numModules = workSheet.Dimension.Rows;

        var modules = new List<TrainingModule>();

        for (int row = 2; row <= numModules; row++)
        {
            var entry = TrainingModule.Create(
                new TrainingModuleId(),
                workSheet.Cells[row, 1].GetCellValue<string>(),
                (TrainingModuleExpiryFrequency)workSheet.Cells[row, 2].GetCellValue<int>(),
                workSheet.Cells[row, 3].GetCellValue<string>(),
                workSheet.Cells[row, 4].GetCellValue<bool>());

            modules.Add(entry);
        }

        var numStaff = workSheet.Dimension.Columns;

        for (int column = 5; column <= numStaff; column++)
        {
            for (int row = 2; row <= numModules; row++)
            {
                var completed = workSheet.Cells[row, column].GetCellValue<string>();
                if (string.IsNullOrWhiteSpace(completed))
                    continue;

                var dateCompleted = DateTime.Parse(completed);

                var moduleName = workSheet.Cells[row, 1].GetCellValue<string>();
                var module = modules.FirstOrDefault(entry => entry.Name == moduleName);
                if (module is null)
                    continue;

                var entry = TrainingCompletion.Create(
                    new TrainingCompletionId(),
                    workSheet.Cells[1, column].GetCellValue<string>(),
                    module.Id);

                entry.SetCompletedDate(dateCompleted);

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
}
