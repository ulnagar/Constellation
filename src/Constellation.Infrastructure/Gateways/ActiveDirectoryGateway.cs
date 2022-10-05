namespace Constellation.Infrastructure.Gateways;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Required for Active Directory Integration")]
public class ActiveDirectoryGateway : IActiveDirectoryGateway
{
    private readonly PrincipalContext _context;
    private readonly ILogger<IActiveDirectoryGateway> _logger;

    public ActiveDirectoryGateway(ILogger<IActiveDirectoryGateway> logger)
    {
        _logger = logger;

        try
        {
            _logger.LogInformation("Attempting to connect to Active Directory");
            _context = new PrincipalContext(ContextType.Domain, "DETNSW.WIN");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to connect to Active Directory: {message}", ex.Message);
        }
    }

    public Task<List<string>> GetLinkedSchoolsFromActiveDirectory(string email)
    {
        var linkedSchools = new List<string>();

        _logger.LogInformation("Searching AD for schools linked to user {email}", email);

        var userAccount = UserPrincipal.FindByIdentity(_context, IdentityType.UserPrincipalName, email);

        if (userAccount == null)
        {
            _logger.LogWarning("Could not find user in AD: {email}", email);
            return Task.FromResult(linkedSchools);
        }

        using (DirectoryEntry adAccount = userAccount.GetUnderlyingObject() as DirectoryEntry)
        {
            // detAttribute12 is the staff attribute that lists linked school codes
            // detAttribute3 is the staff attribute that contains the employee number

            try
            {
                // Get list of users linked schools from AD
                var adAttributeValue = adAccount.Properties["detAttribute12"].Value;

                // If the adAttributeValue is a string, it is a single school link
                // If the adAttributeValue is an array, it is multiple school links

                if (adAttributeValue.GetType() == typeof(string))
                {
                    _logger.LogInformation("Found linked school {code} for user {email}", adAttributeValue as string, email);
                    linkedSchools.Add(adAttributeValue as string);
                }
                else
                {
                    foreach (var entry in adAttributeValue as Array)
                    {
                        _logger.LogInformation("Found linked school {code} for user {email}", entry as string, email);

                        linkedSchools.Add(entry as string);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error searching for linked schools for user {email}: {message}", email, ex.Message);
            }
        }

        return Task.FromResult(linkedSchools);
    }

    public Task<UserTemplateDto> GetUserDetailsFromActiveDirectory(string email)
    {
        var contact = new UserTemplateDto();

        _logger.LogInformation("Searching AD for user details for email {email}", email);

        var userAccount = UserPrincipal.FindByIdentity(_context, IdentityType.UserPrincipalName, email);

        if (userAccount == null)
        {
            _logger.LogWarning("Could not find user in AD: {email}", email);
            return Task.FromResult(contact);
        }

        using (DirectoryEntry adAccount = userAccount.GetUnderlyingObject() as DirectoryEntry)
        {
            // givenName = First Name
            // sn = Last Name

            try
            {
                var givenNameAttribute = adAccount.Properties["givenName"].Value as string;
                contact.FirstName = givenNameAttribute;

                var snAttribute = adAccount.Properties["sn"].Value as string;
                contact.LastName = snAttribute;

                contact.Email = email;

                _logger.LogInformation("Found AD user for email {email}: {user}", email, contact);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error searching for user {email}: {message}", email, ex.Message);
            }
        }

        return Task.FromResult(contact);
    }

    public Task<bool> VerifyUserCredentialsAgainstActiveDirectory(string email, string password)
    {
        _logger.LogInformation("Checking AD user credentials for user {email}", email);

        var result = _context.ValidateCredentials(email, password);

        if (result)
            _logger.LogInformation("User credentials successfully validated for user {email}", email);
        else
            _logger.LogWarning("User credentials could not be validated for user {email}", email);

        return Task.FromResult(result);
    }
}
