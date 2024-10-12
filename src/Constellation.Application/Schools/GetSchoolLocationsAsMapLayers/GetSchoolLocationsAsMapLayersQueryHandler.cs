namespace Constellation.Application.Schools.GetSchoolLocationsAsMapLayers;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using DTOs;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolLocationsAsMapLayersQueryHandler
: IQueryHandler<GetSchoolLocationsAsMapLayersQuery, List<MapLayer>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetSchoolLocationsAsMapLayersQueryHandler(
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _logger = logger
        .ForContext<GetSchoolLocationsAsMapLayersQuery>();
    }

    public async Task<Result<List<MapLayer>>> Handle(GetSchoolLocationsAsMapLayersQuery request, CancellationToken cancellationToken)
    {
        List<MapLayer> layers = new();
        
        MapLayer blueLayer = new() { Colour = "blue", Name = "Students only" };
        MapLayer redLayer = new() { Colour = "red", Name = "Staff only" };
        MapLayer greenLayer = new() { Colour = "green", Name = "Both Students and Staff" };

        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        foreach (School school in schools)
        {
            int studentCount = await _studentRepository.GetCountCurrentStudentsFromSchool(school.Code, cancellationToken);
            int staffCount = await _staffRepository.GetCountCurrentStaffFromSchool(school.Code, cancellationToken);

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
                case 0 when staffCount > 0:
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
