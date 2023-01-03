using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Features.API.Schools.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.API.Schools.Queries
{
    public class GetGraphDataForSchoolQueryHandler : IRequestHandler<GetGraphDataForSchoolQuery, GraphData>
    {
        private readonly IAppDbContext _context;
        private readonly INetworkStatisticsGateway _gateway;

        public GetGraphDataForSchoolQueryHandler(IAppDbContext _context)
        {
            this._context = _context;
        }

        public GetGraphDataForSchoolQueryHandler(IAppDbContext context, INetworkStatisticsGateway gateway)
        {
            _context = context;
            _gateway = gateway;
        }

        public async Task<GraphData> Handle(GetGraphDataForSchoolQuery request, CancellationToken cancellationToken)
        {
            if (_gateway is null)
                return null;

            var school = await _context.Schools
                .Include(school => school.Students)
                .FirstOrDefaultAsync(school => school.Code == request.SchoolCode);

            if (school == null)
                return null;

            var data = await _gateway.GetSiteDetails(request.SchoolCode);
            await _gateway.GetSiteUsage(data, request.Day);

            var returnData = new GraphData
            {
                SiteName = data.SiteName
            };

            // Is the graph data empty?
            if (!data.WANData.Any())
                return returnData;

            // What day is the graph representing?
            var dateDay = data.WANData.First().Time.Date;

            // What day of the cycle is this?
            var cyclicalDay = dateDay.GetDayNumber();

            var periods = new List<TimetablePeriod>();

            // Get the periods for all students at this school
            foreach (var student in school.Students)
            {
                var studentPeriods = await _context.Periods
                    .Where(period => period.OfferingSessions.Any(session => !session.IsDeleted && session.Offering.Enrolments.Any(enrolment => !enrolment.IsDeleted && enrolment.StudentId == student.StudentId)))
                    .ToListAsync();

                periods.AddRange(studentPeriods);
            }

            // Which of these periods are on the right day?
            periods = periods.Where(p => p.Day == cyclicalDay).ToList();

            returnData.Date = dateDay.ToString("D");
            returnData.IntlDate = dateDay.ToString("yyyy-MM-dd");

            foreach (var dataPoint in data.WANData)
            {
                var classSession = periods.Any(p => p.StartTime <= dataPoint.Time.TimeOfDay && p.EndTime >= dataPoint.Time.TimeOfDay);

                var point = new GraphDataPoint
                {
                    Time = dataPoint.Time.ToString("HH:mm"),
                    Lesson = classSession,
                    Networks = new List<GraphDataPointDetail>
                    {
                        new GraphDataPointDetail
                        {
                            Network = "WAN",
                            Connection = data.WANBandwidth.IfNotNull(c => c / 1000000),
                            Inbound = decimal.Round(dataPoint.WANInbound, 2),
                            Outbound = decimal.Round(dataPoint.WANOutbound, 2),
                        },
                        new GraphDataPointDetail
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
}
