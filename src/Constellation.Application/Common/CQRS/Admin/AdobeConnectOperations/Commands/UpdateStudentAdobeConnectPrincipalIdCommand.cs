using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Admin.AdobeConnectOperations.Commands
{
    public class UpdateStudentAdobeConnectPrincipalIdCommand : IRequest
    {
        public string StudentId { get; set; }
        public string PrincipalId { get; set; }
    }

    public class UpdateStudentAdobeConnectPrincipalIdCommandValidator : AbstractValidator<UpdateStudentAdobeConnectPrincipalIdCommand>
    {
        public UpdateStudentAdobeConnectPrincipalIdCommandValidator()
        {
            RuleFor(command => command.StudentId).NotEmpty();
            RuleFor(command => command.PrincipalId).NotEmpty();
        }
    }

    public class UpdateStudentAdobeConnectPrincipalIdCommandHandler : IRequestHandler<UpdateStudentAdobeConnectPrincipalIdCommand>
    {
        private readonly IAppDbContext _context;

        public UpdateStudentAdobeConnectPrincipalIdCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(UpdateStudentAdobeConnectPrincipalIdCommand request, CancellationToken cancellationToken)
        {
            var record = new Student { StudentId = request.StudentId, AdobeConnectPrincipalId = request.PrincipalId };
            _context.Attach(record);
            _context.Entry(record).Property("AdobeConnectPrincipalId").IsModified = true;
            await _context.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
