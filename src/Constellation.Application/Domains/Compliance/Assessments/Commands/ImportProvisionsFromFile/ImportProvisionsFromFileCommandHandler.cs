namespace Constellation.Application.Domains.Compliance.Assessments.Commands.ImportProvisionsFromFile;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Models.Subjects.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.ValueObjects;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Interfaces;
using Models;
using OfficeOpenXml;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ImportProvisionsFromFileCommandHandler
: ICommandHandler<ImportProvisionsFromFileCommand>
{
    private readonly IAssessmentProvisionsCacheService _cacheService;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;
    
    private readonly List<Student> _cachedStudents = [];
    private readonly List<Offering> _cachedOfferings = [];
    private readonly List<Course> _cachedCourses = [];

    public ImportProvisionsFromFileCommandHandler(
        IAssessmentProvisionsCacheService cacheService,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _cacheService = cacheService;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<Result> Handle(ImportProvisionsFromFileCommand request, CancellationToken cancellationToken)
    {
        List<AssessmentProvision> importData = [];

        ExcelPackage excel = new(request.ImportFile);
        ExcelWorksheet sheet = excel.Workbook.Worksheets[0];

        int numRows = sheet.Dimension.Rows;

        for (int row = 2; row <= numRows; row++)
        {
            string stringSRN = sheet.Cells[row, 1].GetCellValue<string>();
            string examName = sheet.Cells[row, 2].GetCellValue<string>();
            string stringOffering = sheet.Cells[row, 5].GetCellValue<string>();
            List<string> adjustments = [];

            int numCols = sheet.Dimension.Columns;

            for (int col = 6; col <= numCols; col++)
            {
                string adjustment = sheet.Cells[row, col].GetCellValue<string>();

                if (!string.IsNullOrWhiteSpace(adjustment))
                    adjustments.Add(sheet.Cells[1, col].GetCellValue<string>());
            }

            Result<StudentReferenceNumber> srnResult = StudentReferenceNumber.Create(stringSRN);

            if (srnResult.IsFailure)
                return Result.Failure(new("ImportError", $"Error importing at row {row} due to invalid SRN"));

            Result<OfferingName> offeringNameResult = OfferingName.FromValue(stringOffering);

            if (offeringNameResult.IsFailure)
                return Result.Failure(new("ImportError", $"Error importing at row {row} due to invalid Offering Name"));

            importData.Add(new()
            {
                StudentReferenceNumber = srnResult.Value,
                ExamName = examName,
                OfferingName = offeringNameResult.Value,
                Adjustments = adjustments
            });
        }
        
        IEnumerable<IGrouping<StudentReferenceNumber, AssessmentProvision>> groupedByStudent = importData.GroupBy(entry => entry.StudentReferenceNumber);

        foreach (IGrouping<StudentReferenceNumber, AssessmentProvision> studentReferenceNumber in groupedByStudent)
        {
            Student student = _cachedStudents.FirstOrDefault(student => student.StudentReferenceNumber == studentReferenceNumber.Key);

            if (student is null)
            {
                student = await _studentRepository.GetBySRN(studentReferenceNumber.Key, cancellationToken);

                if (student is null)
                    return Result.Failure<List<StudentProvisions>>(StudentErrors.NotFoundBySRN(studentReferenceNumber.Key));

                _cachedStudents.Add(student);
            }

            List<StudentProvisions.OfferingProvision> offeringProvisions = [];

            foreach (AssessmentProvision provision in studentReferenceNumber)
            {
                Offering offering = _cachedOfferings.FirstOrDefault(offering => offering.Name == provision.OfferingName);

                if (offering is null)
                {
                    offering = await _offeringRepository.GetFromYearAndName(_dateTime.Today.Year, provision.OfferingName, cancellationToken);

                    if (offering is null)
                        return Result.Failure<List<StudentProvisions>>(OfferingErrors.NotFoundForName(provision.OfferingName));

                    _cachedOfferings.Add(offering);
                }

                Course course = _cachedCourses.FirstOrDefault(course => course.Id == offering.CourseId);

                if (course is null)
                {
                    course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

                    if (course is null)
                        return Result.Failure<List<StudentProvisions>>(CourseErrors.NotFound(offering.CourseId));

                    _cachedCourses.Add(course);
                }

                offeringProvisions.Add(new(
                    offering.Id,
                    course.Name,
                    offering.Name,
                    provision.ExamName,
                    provision.Adjustments));
            }

            _cacheService.Insert(new StudentProvisions(
                student.Id,
                student.Name,
                student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
                offeringProvisions));
        }

        return Result.Success();
    }

    internal sealed class AssessmentProvision
    {
        public StudentReferenceNumber StudentReferenceNumber { get; set; }
        public OfferingName OfferingName { get; set; }
        public string ExamName { get; set; }
        public List<string> Adjustments { get; set; }
    }
}

