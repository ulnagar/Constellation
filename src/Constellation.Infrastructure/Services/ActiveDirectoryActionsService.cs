using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CA1416 // Validate platform compatibility
namespace Constellation.Infrastructure.Services
{
    public class ActiveDirectoryActionsService : IActiveDirectoryActionsService, IScopedService
    {
        private readonly IAppDbContext _context;
        private readonly PrincipalContext _adContext;

        public ActiveDirectoryActionsService(IAppDbContext context)
        {
            _context = context;
            try
            {
                _adContext = new PrincipalContext(ContextType.Domain, "DETNSW.WIN");
            }
            catch (Exception)
            {
                //
            }
        }

        public async Task<List<string>> GetLinkedSchoolsFromAD(string emailAddress)
        {
            var linkedSchools = new List<string>();

            var userAccount = UserPrincipal.FindByIdentity(_adContext, IdentityType.UserPrincipalName, emailAddress);
            using (DirectoryEntry adAccount = userAccount.GetUnderlyingObject() as DirectoryEntry)
            {
                // detAttribute12 is the staff attribute that lists linked school codes
                // detAttribute3 is the staff attribute that contains the employee number

                try
                {
                    // Get list of users linked schools from AD
                    var adAttributeValue = adAccount.Properties["detAttribute12"].Value;
                    var adSchoolList = new List<string>();

                    // If the adAttributeValue is a string, it is a single school link
                    // If the adAttributeValue is an array, it is multiple school links

                    if (adAttributeValue.GetType() == typeof(string))
                        adSchoolList.Add(adAttributeValue as string);
                    else
                    {
                        foreach (var entry in adAttributeValue as Array)
                        {
                            adSchoolList.Add(entry as string);
                        }
                    }

                    // Check each school against the DB to ensure it is an active partner school with students
                    // Add any matching entries to the user claims
                    foreach (var code in adSchoolList)
                    {
                        var isPartnerSchool = await _context.Schools.AnyAsync(school => school.Code == code && school.Students.Any(student => !student.IsDeleted));
                        var isInList = linkedSchools.Any(entry => entry == code);

                        if (isPartnerSchool && !isInList)
                            linkedSchools.Add(code);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return linkedSchools;
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
