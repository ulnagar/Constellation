namespace Constellation.Application.Features.API.Schools.Queries;

using Constellation.Core.Models.Students.Repositories;
using Core.Models;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Students;
using DTOs;
using Extensions;
using Interfaces.Gateways;
using Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetGraphDataForSchoolQueryHandler : IRequestHandler<GetGraphDataForSchoolQuery, GraphData>
{
    private readonly INetworkStatisticsGateway _gateway;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly IStudentRepository _studentRepository;

    public GetGraphDataForSchoolQueryHandler(
        INetworkStatisticsGateway gateway,
        ISchoolRepository schoolRepository,
        IOfferingRepository offeringRepository,
        ITimetablePeriodRepository periodRepository,
        IStudentRepository studentRepository)
    {
        _gateway = gateway;
        _schoolRepository = schoolRepository;
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _studentRepository = studentRepository;
    }

    public async Task<GraphData> Handle(GetGraphDataForSchoolQuery request, CancellationToken cancellationToken)
    {
        if (_gateway is null)
            return null;

        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school == null)
            return null;

        List<Student> students = await _studentRepository.GetCurrentStudentsFromSchool(request.SchoolCode, cancellationToken);

        NetworkStatisticsSiteDto data = await _gateway.GetSiteDetails(request.SchoolCode);
        await _gateway.GetSiteUsage(data, request.Day);

        GraphData returnData = new()
        {
            SiteName = data.SiteName
        };

        // Is the graph data empty?
        if (!data.WANData.Any())
            return returnData;

        // What day is the graph representing?
        DateTime dateDay = data.WANData.First().Time.Date;

        // What day of the cycle is this?
        int cyclicalDay = dateDay.GetDayNumber();

        List<TimetablePeriod> periods = new();

        // Get the periods for all students at this school
        foreach (Student student in students)
        {
            List<Offering> offerings = await _offeringRepository.GetByStudentId(student.StudentId, cancellationToken);

            List<int> periodIds = offerings
                .SelectMany(offering =>
                    offering.Sessions
                        .Where(session => !session.IsDeleted)
                        .Select(session => session.PeriodId))
                .ToList();

            List<TimetablePeriod> studentPeriods = await _periodRepository.GetListFromIds(periodIds, cancellationToken);

            periods.AddRange(studentPeriods);
        }

        // Which of these periods are on the right day?
        periods = periods.Where(p => p.Day == cyclicalDay).ToList();

        returnData.Date = dateDay.ToString("D");
        returnData.IntlDate = dateDay.ToString("yyyy-MM-dd");

        foreach (NetworkStatisticsSiteDto.PointOfTimeUsage dataPoint in data.WANData)
        {
            bool classSession = periods.Any(p => p.StartTime <= dataPoint.Time.TimeOfDay && p.EndTime >= dataPoint.Time.TimeOfDay);

            GraphDataPoint point = new()
            {
                Time = dataPoint.Time.ToString("HH:mm"),
                Lesson = classSession,
                Networks = new List<GraphDataPointDetail>
                {
                    new()
                    {
                        Network = "WAN",
                        Connection = data.WANBandwidth.IfNotNull(c => c / 1000000),
                        Inbound = decimal.Round(dataPoint.WANInbound, 2),
                        Outbound = decimal.Round(dataPoint.WANOutbound, 2),
                    },
                    new()
                    {
                        Network = "INT",
                        Connection = data.INTBandwidth.IfNotNull(c => c / 1000000),
                        Inbound = decimal.Round(dataPoint.INTInbound, 2),
                        Outbound = decimal.Round(dataPoint.INTOutbound, 2),
                    }
                }
            };

            if (returnData.Data.All(d => d.Time != point.Time))
                returnData.Data.Add(point);
        }

        return returnData;
    }
}