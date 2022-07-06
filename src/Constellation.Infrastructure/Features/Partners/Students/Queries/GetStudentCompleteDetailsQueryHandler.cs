using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.DTOs;
using Constellation.Application.Features.Partners.Students.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Students.Queries
{
    public class GetStudentCompleteDetailsQueryHandler : IRequestHandler<GetStudentCompleteDetailsQuery, StudentCompleteDetailsDto>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStudentCompleteDetailsQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<StudentCompleteDetailsDto> Handle(GetStudentCompleteDetailsQuery request, CancellationToken cancellationToken)
        {
            var viewModel = new StudentCompleteDetailsDto();

            viewModel.Student = await _context.Students
                .Where(student => student.StudentId == request.StudentId)
                .ProjectTo<StudentCompleteDetailsDto.StudentDetails>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);

            viewModel.Absences = await _context.Absences
                .Where(absence => absence.StudentId == request.StudentId && absence.Date.Year == DateTime.Today.Year)
                .ProjectTo<StudentCompleteDetailsDto.Absence>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            viewModel.Enrolments = await _context.Enrolments
                .Where(enrolment => enrolment.StudentId == request.StudentId && !enrolment.DateDeleted.HasValue && enrolment.Offering.EndDate > DateTime.Today)
                .Select(enrolment => enrolment.Offering)
                .ProjectTo<StudentCompleteDetailsDto.Offering>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            viewModel.Equipment = await _context.DeviceAllocations
                .Where(allocation => allocation.StudentId == request.StudentId)
                .ProjectTo<StudentCompleteDetailsDto.Allocation>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            viewModel.Family = await _context.StudentFamilies
                .FirstOrDefaultAsync(family => family.Students.Any(student => student.StudentId == request.StudentId), cancellationToken);

            viewModel.Sessions = await _context.Enrolments
                .Where(enrolment => enrolment.StudentId == request.StudentId && !enrolment.DateDeleted.HasValue && enrolment.Offering.EndDate > DateTime.Today)
                .SelectMany(enrolment => enrolment.Offering.Sessions)
                .ProjectTo<StudentCompleteDetailsDto.Session>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return viewModel;
        }
    }
}
