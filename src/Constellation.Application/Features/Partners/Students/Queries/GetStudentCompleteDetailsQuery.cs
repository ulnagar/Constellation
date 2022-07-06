using Constellation.Application.DTOs;
using MediatR;

namespace Constellation.Application.Features.Partners.Students.Queries
{
    public class GetStudentCompleteDetailsQuery : IRequest<StudentCompleteDetailsDto>
    {
        public string StudentId { get; set; }   
    }
}