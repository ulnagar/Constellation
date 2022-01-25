using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC operations
    public class SchoolService : ISchoolService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SchoolService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceOperationResult<School>> CreateSchool(SchoolDto schoolResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<School>();

            if (await _unitOfWork.Schools.AnyWithId(schoolResource.Code))
            {
                result.Success = false;
                result.Errors.Add($"A school with that ID already exists!");

                return result;
            }

            var school = new School()
            {
                Code = schoolResource.Code,
                Name = schoolResource.Name,
                Address = schoolResource.Address,
                Town = schoolResource.Town,
                State = schoolResource.State,
                PostCode = schoolResource.PostCode,
                PhoneNumber = schoolResource.PhoneNumber,
                FaxNumber = schoolResource.FaxNumber,
                EmailAddress = schoolResource.EmailAddress,
                Division = schoolResource.Division,
                HeatSchool = schoolResource.HeatSchool,
                Electorate = schoolResource.Electorate,
                PrincipalNetwork = schoolResource.PrincipalNetwork,
                TimetableApplication = schoolResource.TimetableApplication,
                RollCallGroup = schoolResource.RollCallGroup
            };

            _unitOfWork.Add(school);

            result.Success = true;
            result.Entity = school;

            return result;
        }

        public async Task<ServiceOperationResult<School>> UpdateSchool(string code, SchoolDto schoolResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<School>();

            // Validate entries
            var school = await _unitOfWork.Schools.ForEditAsync(code);

            if (school == null)
            {
                result.Success = false;
                result.Errors.Add($"A school with that ID cannot be found!");

                return result;
            }

            if (!string.IsNullOrWhiteSpace(schoolResource.Name))
                school.Name = schoolResource.Name;

            if (!string.IsNullOrWhiteSpace(schoolResource.Address))
                school.Address = schoolResource.Address;

            if (!string.IsNullOrWhiteSpace(schoolResource.Town))
                school.Town = schoolResource.Town;

            if (!string.IsNullOrWhiteSpace(schoolResource.State))
                school.State = schoolResource.State;

            if (!string.IsNullOrWhiteSpace(schoolResource.PostCode))
                school.PostCode = schoolResource.PostCode;

            if (!string.IsNullOrWhiteSpace(schoolResource.PhoneNumber))
                school.PhoneNumber = schoolResource.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(schoolResource.FaxNumber))
                school.FaxNumber = schoolResource.FaxNumber;

            if (!string.IsNullOrWhiteSpace(schoolResource.EmailAddress))
                school.EmailAddress = schoolResource.EmailAddress;

            if (!string.IsNullOrWhiteSpace(schoolResource.Division))
                school.Division = schoolResource.Division;

            if (!string.IsNullOrWhiteSpace(schoolResource.Electorate))
                school.Electorate = schoolResource.Electorate;

            if (school.HeatSchool != schoolResource.HeatSchool)
                school.HeatSchool = schoolResource.HeatSchool;

            if (!string.IsNullOrWhiteSpace(schoolResource.PrincipalNetwork))
                school.PrincipalNetwork = schoolResource.PrincipalNetwork;

            if (!string.IsNullOrWhiteSpace(schoolResource.TimetableApplication))
                school.TimetableApplication = schoolResource.TimetableApplication;

            if (!string.IsNullOrWhiteSpace(schoolResource.RollCallGroup))
                school.RollCallGroup = schoolResource.RollCallGroup;

            result.Success = true;
            result.Entity = school;

            return result;
        }
    }
}
