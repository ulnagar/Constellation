using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetStudentSentralIdQueryHandler : IRequestHandler<GetStudentSentralIdQuery, string>
    {
        private readonly IAppDbContext _context;
        private readonly ISentralGateway _sentralGateway;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;

        public GetStudentSentralIdQueryHandler(IAppDbContext context, ISentralGateway sentralGateway, IMapper mapper, IEmailService emailService, ILogger logger)
        {
            _context = context;
            _sentralGateway = sentralGateway;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger.ForContext<GetStudentSentralIdQuery>();
        }

        public async Task<string> Handle(GetStudentSentralIdQuery request, CancellationToken cancellationToken)
        {
            var student = await _context.Students
                .Where(student => student.StudentId == request.StudentId)
                .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return null;

            var sentralId = student.SentralStudentId;

            try
            {
                sentralId = await _sentralGateway.GetSentralStudentIdFromSRN(request.StudentId, ((int)student.CurrentGrade).ToString());
            }
            catch (Exception ex)
            {
                _logger.Error("Error trying to retrieve student ({student}) identifier from Sentral: {message}", student.Name, ex.Message, ex);
            }

            if (string.IsNullOrWhiteSpace(sentralId))
            {
                // Send warning email to Technology Support Team
                await _emailService.SendAdminAbsenceSentralAlert(student.Name);
                return null;
            }

            return sentralId;
        }

        private class StudentDto : IMapFrom<Student>
        {
            public string SentralStudentId { get; set; }
            public Grade CurrentGrade { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Name => $"{FirstName} {LastName}";
        }
    }
}
