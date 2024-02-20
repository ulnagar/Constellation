namespace Constellation.Infrastructure.Services;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

#pragma warning disable CA1416 // Validate platform compatibility
public sealed class ActiveDirectoryActionsService : IActiveDirectoryActionsService, IDisposable
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;
    private readonly PrincipalContext _adContext;

    public ActiveDirectoryActionsService(
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<IActiveDirectoryActionsService>();

        try
        {
            _adContext = new PrincipalContext(ContextType.Domain, "DETNSW.WIN");
        }
        catch (Exception)
        {
            _logger.Warning("Failed to connect to Active Directory!");
        }
    }

    public async Task<List<string>> GetLinkedSchoolsFromAD(string emailAddress)
    {
        List<string> linkedSchools = new List<string>();

        UserPrincipal userAccount = UserPrincipal.FindByIdentity(_adContext, IdentityType.UserPrincipalName, emailAddress);
            
        if (userAccount is null)
            return null;

        using DirectoryEntry adAccount = userAccount.GetUnderlyingObject() as DirectoryEntry;
        if (adAccount is null)
            return null;
            
        // detAttribute12 is the staff attribute that lists linked school codes
        // detAttribute3 is the staff attribute that contains the employee number

        try
        {
            // Get list of users linked schools from AD
            object adAttributeValue = adAccount.Properties["detAttribute12"].Value;
            List<string> adSchoolList = new List<string>();

            // If the adAttributeValue is a string, it is a single school link
            // If the adAttributeValue is an array, it is multiple school links
            switch (adAttributeValue)
            {
                case null:
                    return null;
                case string attributeValue:
                    adSchoolList.Add(attributeValue);
                    break;
                default:
                    adSchoolList.AddRange(from object entry in (Array)adAttributeValue select entry as string);
                    break;
            }

            // Check each school against the DB to ensure it is an active partner school with students
            // Add any matching entries to the user claims
            foreach (string code in adSchoolList)
            {
                bool isPartnerSchool = await _schoolRepository.IsPartnerSchoolWithStudents(code);

                bool isInList = linkedSchools.Any(entry => entry == code);

                if (isPartnerSchool && !isInList)
                    linkedSchools.Add(code);
            }

            return linkedSchools;
        }
        catch (Exception ex)
        {
            _logger
                .ForContext(nameof(Exception), ex, true)
                .Warning("Error when trying to retrieve Users linked school codes");

            return null;
        }
    }

    public (string FirstName, string LastName) GetUserDetailsFromAD(string emailAddress)
    {
        UserPrincipal userAccount = UserPrincipal.FindByIdentity(_adContext, IdentityType.UserPrincipalName, emailAddress);

        if (userAccount is null)
            return (null, null);

        using DirectoryEntry adAccount = userAccount.GetUnderlyingObject() as DirectoryEntry;
        // givenName = First Name
        // sn = Last Name

        if (adAccount is null)
            return (null, null);

        try
        {
            string givenNameAttribute = adAccount.Properties["givenName"].Value as string;
            string snAttribute = adAccount.Properties["sn"].Value as string;

            return (givenNameAttribute, snAttribute);
        }
        catch (Exception ex)
        {
            _logger
                .ForContext(nameof(Exception), ex, true)
                .Warning("Error when trying to retrieve User details");

            return (null, null);
        }
    }

    public void Dispose() => _adContext?.Dispose();
}
#pragma warning restore CA1416 // Validate platform compatibility
