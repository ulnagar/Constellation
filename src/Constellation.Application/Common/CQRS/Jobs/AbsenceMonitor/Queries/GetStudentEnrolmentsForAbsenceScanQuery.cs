using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Jobs.AbsenceMonitor.Queries
{
    public class GetStudentEnrolmentsForAbsenceScanQuery : IRequest<ICollection<StudentEnrolmentsForAbsenceScan>>
    {
        public string StudentId { get; set; }
        public DateTime InstanceDate { get; set; }
        public int PeriodDay { get; set; }
    }

    public class StudentEnrolmentsForAbsenceScan : IMapFrom<CourseOffering>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GetStudentEnromentsForAbsenceScanQueryHandler : IRequestHandler<GetStudentEnrolmentsForAbsenceScanQuery, ICollection<StudentEnrolmentsForAbsenceScan>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStudentEnromentsForAbsenceScanQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StudentEnrolmentsForAbsenceScan>> Handle(GetStudentEnrolmentsForAbsenceScanQuery request, CancellationToken cancellationToken)
        {
            var courseEnrolments = await _context.Enrolments
                .Where(enrolment => enrolment.StudentId == request.StudentId &&
                    enrolment.DateCreated < request.InstanceDate &&
                    (!enrolment.IsDeleted || enrolment.DateDeleted.Value.Date > request.InstanceDate) &&
                    enrolment.Offering.EndDate > request.InstanceDate &&
                    enrolment.Offering.Sessions.Any(session =>
                        session.DateCreated < request.InstanceDate &&
                        (!session.IsDeleted || session.DateDeleted.Value.Date > request.InstanceDate) &&
                        session.Period.Day == request.PeriodDay))
                .Select(enrolment => enrolment.Offering)
                .ProjectTo<StudentEnrolmentsForAbsenceScan>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken: cancellationToken);
                
            return courseEnrolments;
        }
    }
}
