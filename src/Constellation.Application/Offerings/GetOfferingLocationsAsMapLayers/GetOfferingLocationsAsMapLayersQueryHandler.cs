namespace Constellation.Application.Offerings.GetOfferingLocationsAsMapLayers;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetOfferingLocationsAsMapLayersQueryHandler
    : IQueryHandler<GetOfferingLocationsAsMapLayersQuery, List<MapLayer>>
{
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetOfferingLocationsAsMapLayersQueryHandler(
        IEnrolmentRepository enrolmentRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _enrolmentRepository = enrolmentRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<GetOfferingLocationsAsMapLayersQuery>();
    }

    public async Task<Result<List<MapLayer>>> Handle(GetOfferingLocationsAsMapLayersQuery request, CancellationToken cancellationToken)
    {
        List<string> SchoolCodes = new();

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByOfferingId(request.OfferingId, cancellationToken);

        foreach (Enrolment enrolment in enrolments)
        {
            Student student = await _studentRepository.GetById(enrolment.StudentId, cancellationToken);

            if (!SchoolCodes.Contains(student.SchoolCode))
                SchoolCodes.Add(student.SchoolCode);
        }

        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);
        
        if (offering is null)
        {
            _logger
                .ForContext(nameof(GetOfferingLocationsAsMapLayersQuery), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(request.OfferingId), true)
                .Warning("Could not retrieve Offering locations for map");

            return Result.Failure<List<MapLayer>>(OfferingErrors.NotFound(request.OfferingId));
        }

        foreach (TeacherAssignment assignment in offering.Teachers)
        {
            Staff staffMember = await _staffRepository.GetById(assignment.StaffId, cancellationToken);

            if (!SchoolCodes.Contains(staffMember.SchoolCode))
                SchoolCodes.Add(staffMember.SchoolCode);
        }

        List<MapLayer> layers = _schoolRepository.GetForMapping(SchoolCodes).ToList();

        return layers;
    }

}
