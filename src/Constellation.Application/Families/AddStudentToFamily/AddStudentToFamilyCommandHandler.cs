namespace Constellation.Application.Families.AddStudentToFamily;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models.Families;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddStudentToFamilyCommandHandler
    : ICommandHandler<AddStudentToFamilyCommand>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddStudentToFamilyCommandHandler(
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork)
    {
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddStudentToFamilyCommand request, CancellationToken cancellationToken)
    {
        Family family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
            return Result.Failure(DomainErrors.Families.Family.NotFound(request.FamilyId));

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
            return Result.Failure(StudentErrors.NotFound(request.StudentId));

        Result<StudentFamilyMembership> result = family.AddStudent(request.StudentId, student.StudentReferenceNumber, false);

        if (result.IsSuccess)
            await _unitOfWork.CompleteAsync(cancellationToken);

        return result;
    }
}
