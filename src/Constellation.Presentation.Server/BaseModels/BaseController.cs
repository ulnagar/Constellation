﻿using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.BaseModels
{
    public class BaseController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BaseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected async Task<T> CreateViewModel<T>() where T : BaseViewModel, new()
        {
            T viewModel = new T();
            await UpdateViewModel(viewModel);

            return viewModel;
        }

        protected async Task UpdateViewModel<T>(T viewModel) where T : BaseViewModel
        {
            viewModel.Classes = await GetClasses();
        }

        protected async Task<IDictionary<string, int>> GetClasses()
        {
            var result = new Dictionary<string, int>();

            var user = (ClaimsIdentity)User.Identity;
            var claims = user.Claims;
            var staffId = claims.FirstOrDefault(claim => claim.Type == AppUser.StaffMemberId);

            if (staffId != null && !string.IsNullOrWhiteSpace(staffId.Value))
            {
                var entries = await _unitOfWork.CourseOfferings.AllForTeacherAsync(staffId.Value);

                foreach (var entry in entries)
                {
                    result.Add(entry.Name, entry.Id);
                }
            }

            return result;
        }
    }
}