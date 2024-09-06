namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Interfaces.Repositories;
using BaseModels;
using Constellation.Application.StaffMembers.CreateStaffMember;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Enums;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using MediatR;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(
        ISender mediator,
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IStaffRepository staffRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _staffRepository = staffRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }


    public async Task OnGet()
    {
        School? school = await _schoolRepository.GetById("8912");

        if (school is null)
        {
            school = new()
            {
                Code = "8912",
                Name = "Aurora College",
                Address = "100 Eton Road",
                Town = "Lindfield",
                State = "NSW",
                PostCode = "2070",
                EmailAddress = "auroracoll-h.school@det.nsw.edu.au",
                PhoneNumber = "1300287629"
            };

            _schoolRepository.Insert(school);

            await _unitOfWork.CompleteAsync();
        }

        Result<StudentReferenceNumber> studentReferenceNumber = StudentReferenceNumber.Create("1234567890");

        Student student = await _studentRepository.GetBySRN(studentReferenceNumber.Value);

        if (student is null)
        {
            Result<Name> name = Name.Create("John", "Johnny", "Doe");

            Result<EmailAddress> email = EmailAddress.Create("john.doe13@det.nsw.edu.au");

            Result<Student> studentRequest = Student.Create(
                studentReferenceNumber.Value,
                name.Value,
                email.Value,
                Grade.Y09,
                school,
                2024,
                Gender.Male,
                _dateTime);

            _studentRepository.Insert(studentRequest.Value);

            await _unitOfWork.CompleteAsync();

            student = studentRequest.Value;
        }

        Staff staff = await _staffRepository.GetById("1206070");

        if (staff is null)
        {
            // Create new student
            CreateStaffMemberCommand createCommand = new(
                "1206070",
                "Ben",
                "Hillsley",
                "benjamin.hillsley",
                "8912",
                false);

            Result createResult = await _mediator.Send(createCommand);

            staff = await _staffRepository.GetById("1206070");
        }
    }
}