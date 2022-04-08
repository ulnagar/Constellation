using Constellation.Application.Common.ValidationRules;
using Constellation.Application.Features.Partners.Schools.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Schools.Commands
{
    public class UpsertSchoolHandler : IRequestHandler<UpsertSchool, ValidateableResponse>
    {
        private readonly IAppDbContext _context;

        public UpsertSchoolHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ValidateableResponse> Handle(UpsertSchool request, CancellationToken cancellationToken)
        {
            var entity = await _context.Schools.SingleOrDefaultAsync(school => school.Code == request.Code, cancellationToken);
            var newSchool = false;

            if (entity == null)
            {
                entity = new School();
                newSchool = true;
            }

            entity.Code = request.Code;
            entity.Name = request.Name;
            entity.Address = request.Address;
            entity.Town = request.Town;
            entity.State = request.State;
            entity.PostCode = request.PostCode;
            entity.EmailAddress = request.EmailAddress;
            entity.PhoneNumber = request.PhoneNumber;
            entity.Website = request.Website;
            entity.Longitude = request.Longitude;
            entity.Latitude = request.Latitude;
            entity.HeatSchool = request.LateOpening;

            if (newSchool)
                _context.Schools.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return new ValidateableResponse();
        }
    }
}
