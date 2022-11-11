using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.DependencyInjection;
using LinqKit;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC operations
    public class ExcelService : IExcelService, IScopedService
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

        public async Task<MemoryStream> CreateAbsencesFile(ICollection<AbsenceExportDto> exportAbsences, string title)
        {
            var excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet 1");
            var pageTitle = workSheet.Cells[1, 1].RichText.Add(title);
            pageTitle.Bold = true;
            pageTitle.Size = 16;

            workSheet.Cells[2, 1].LoadFromCollection(exportAbsences, true);
            workSheet.Cells[2, 6, workSheet.Dimension.Rows, 6].Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[2, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

            var memoryStream = new MemoryStream();
            await excel.SaveAsAsync(memoryStream);
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
            workSheet.Cells[3, 1].Value = $"Link to module: {new Uri(data.Url)}";

            workSheet.Cells[5, 1].LoadFromCollection(data.Completions, opt =>
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
                    completion.GetProperty("ExpiryCountdown"),
                    completion.GetProperty("CompletedDate")
                };
            });

            workSheet.Cells[6, 6, workSheet.Dimension.Rows, 6].ForEach(range => UpdateDefaultDateDataForRange(range));
            workSheet.Cells[5, 6, workSheet.Dimension.Rows, 6].Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[5, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns].AutoFitColumns();

            // Highlight overdue entries
            var dataRange = new ExcelAddress(6, 1, workSheet.Dimension.Rows, workSheet.Dimension.Columns);

            var formatNeverCompleted = workSheet.ConditionalFormatting.AddExpression(dataRange);
            formatNeverCompleted.Formula = "=$E6 = \"\"";
            formatNeverCompleted.Style.Fill.BackgroundColor.Color = Color.Gray;
            formatNeverCompleted.Style.Font.Color.Color = Color.White;
            formatNeverCompleted.StopIfTrue = true;

            var formatOverdue = workSheet.ConditionalFormatting.AddExpression(dataRange);
            formatOverdue.Formula = "=$E6 < 1";
            formatOverdue.Style.Fill.BackgroundColor.Color = Color.Red;
            formatOverdue.Style.Font.Color.Color = Color.White;
            formatOverdue.StopIfTrue = true;

            var formatSoonExpire = workSheet.ConditionalFormatting.AddExpression(dataRange);
            formatSoonExpire.Formula = "=$E6 < 14";
            formatSoonExpire.Style.Fill.BackgroundColor.Color = Color.Yellow;
            formatSoonExpire.StopIfTrue = true;

            // Freeze top rows
            workSheet.View.FreezePanes(6, 1);

            var memoryStream = new MemoryStream();
            await excel.SaveAsAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        private void UpdateDefaultDateDataForRange(ExcelRangeBase range)
        {
            for (int r = 0; r < range.Rows; r++)
            {
                if (range.GetCellValue<DateTime>(r, 0) == DateTime.MinValue)
                {
                    range.SetCellValue(r, -1, string.Empty);
                    range.SetCellValue(r, 0, string.Empty);
                }
            }
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
}
