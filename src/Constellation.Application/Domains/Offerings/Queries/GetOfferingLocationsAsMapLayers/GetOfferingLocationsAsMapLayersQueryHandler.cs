namespace Constellation.Application.Domains.Offerings.Queries.GetOfferingLocationsAsMapLayers;

using Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Core.Models;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Enrolments.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using DTOs;
using Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        List<string> studentSchoolCodes = new();
        List<string> staffSchoolCodes = new();

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByOfferingId(request.OfferingId, cancellationToken);

        foreach (Enrolment enrolment in enrolments)
        {
            Student student = await _studentRepository.GetById(enrolment.StudentId, cancellationToken);

            if (student is null)
                continue;

            SchoolEnrolment schoolEnrolment = student.CurrentEnrolment;

            if (schoolEnrolment is null)
                continue;

            if (!studentSchoolCodes.Contains(schoolEnrolment.SchoolCode))
                studentSchoolCodes.Add(schoolEnrolment.SchoolCode);
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

            if (!staffSchoolCodes.Contains(staffMember.SchoolCode))
                staffSchoolCodes.Add(staffMember.SchoolCode);
        }

        List<string> schoolCodes = studentSchoolCodes.ToList();

        foreach (string schoolCode in staffSchoolCodes)
        {
            if (!schoolCodes.Contains(schoolCode))
                schoolCodes.Add(schoolCode);
        }

        List<MapLayer> layers = new();

        List<School> schools = await _schoolRepository.GetListFromIds(schoolCodes, cancellationToken);

        MapLayer blueLayer = new() { Colour = "blue", Name = "Students only" };
        MapLayer redLayer = new() { Colour = "red", Name = "Staff only" };
        MapLayer greenLayer = new() { Colour = "green", Name = "Both Students and Staff" };

        foreach (var school in schools)
        {
            int studentCount = await _studentRepository.GetCountCurrentStudentsFromSchool(school.Code, cancellationToken);
            int staffCount = school.Staff.Count(entry => !entry.IsDeleted);

            MapItem marker = new()
            {
                SchoolName = school.Name,
                Latitude = school.Latitude,
                Longitude = school.Longitude,
                StudentCount = studentCount,
                StaffCount = staffCount
            };

            switch (studentCount)
            {
                case > 0 when staffCount > 0:
                    greenLayer.AddMarker(marker);
                    break;
                case  0 when staffCount > 0:
                    redLayer.AddMarker(marker);
                    break;
                case > 0 when staffCount == 0:
                    blueLayer.AddMarker(marker);
                    break;
            }
        }

        layers.Add(blueLayer);
        layers.Add(greenLayer);
        layers.Add(redLayer);
        
        return layers;
    }

}
