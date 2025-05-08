namespace Constellation.Application.Domains.Edval.Events.EdvalStudentsUpdated;

using Abstractions.Messaging;
using Constellation.Core.Models.Edval;
using Constellation.Core.Models.Edval.Enums;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.ValueObjects;
using Constellation.Core.Shared;
using Core.Enums;
using Core.Models.Edval.Events;
using Core.Models.Students.Repositories;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CalculateDifferences : IIntegrationEventHandler<EdvalStudentsUpdatedIntegrationEvent>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CalculateDifferences(
        IEdvalRepository edvalRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<EdvalStudentsUpdatedIntegrationEvent>();
    }
    public async Task Handle(EdvalStudentsUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        List<Student> existingStudents = await _studentRepository.GetCurrentStudents(cancellationToken);

        List<EdvalStudent> edvalStudents = await _edvalRepository.GetStudents(cancellationToken);

        foreach (EdvalStudent edvalStudent in edvalStudents)
        {
            Result<StudentReferenceNumber> srn = StudentReferenceNumber.Create(edvalStudent.StudentId);

            if (srn.IsFailure)
            {
                _logger
                    .ForContext(nameof(EdvalStudent), edvalStudent, true)
                    .ForContext(nameof(Error), srn.Error, true)
                    .Warning("Invalid StudentReferenceNumber provided");

                continue;
            }

            Student student = existingStudents.FirstOrDefault(student => student.StudentReferenceNumber == srn.Value);

            if (student is null)
            {
                // Additional student in Edval
                _edvalRepository.Insert(new Difference()
                {
                    Type = EdvalDifferenceType.EdvalStudent,
                    Description = $"{edvalStudent.FirstName} {edvalStudent.LastName} is not enrolled in Constellation"
                });

                continue;
            }

            if (!student.Name.FirstName.Trim().Equals(edvalStudent.FirstName, StringComparison.OrdinalIgnoreCase) &&
                !student.Name.PreferredName.Trim().Equals(edvalStudent.FirstName, StringComparison.OrdinalIgnoreCase))
            {
                _edvalRepository.Insert(new Difference()
                {
                    Type = EdvalDifferenceType.EdvalStudent,
                    Description = $"{student.Name} has a different First Name ({edvalStudent.FirstName}) in Edval"
                });

                continue;
            }

            if (!student.Name.LastName.Trim().Equals(edvalStudent.LastName, StringComparison.OrdinalIgnoreCase))
            {
                _edvalRepository.Insert(new Difference()
                {
                    Type = EdvalDifferenceType.EdvalStudent,
                    Description = $"{student.Name} has a different Last Name ({edvalStudent.LastName}) in Edval"
                });

                continue;
            }

            if (!student.EmailAddress.Email.Equals(edvalStudent.EmailAddress, StringComparison.OrdinalIgnoreCase))
            {
                _edvalRepository.Insert(new Difference()
                {
                    Type = EdvalDifferenceType.EdvalStudent,
                    Description = $"{student.Name} has a different Email Address ({edvalStudent.EmailAddress}) in Edval"
                });

                continue;
            }

            string edvalGradeString = Regex.Match(edvalStudent.Grade, @"\d+").Value;
            Grade edvalGrade = (Grade)Int32.Parse(edvalGradeString);

            if (student.CurrentEnrolment?.Grade != edvalGrade)
            {
                _edvalRepository.Insert(new Difference()
                {
                    Type = EdvalDifferenceType.EdvalStudent,
                    Description = $"{student.Name} has a different Grade ({edvalStudent.Grade}) in Edval"
                });
            }
        }

        foreach (Student student in existingStudents)
        {
            if (edvalStudents.All(edvalStudent => edvalStudent.StudentId != student.StudentReferenceNumber.Number))
            {
                _edvalRepository.Insert(new Difference()
                {
                    Type = EdvalDifferenceType.EdvalStudent,
                    Description = $"{student.Name} is not enrolled in Edval"
                });
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}