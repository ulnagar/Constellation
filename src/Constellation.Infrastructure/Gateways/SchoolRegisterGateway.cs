using Constellation.Application.DTOs;
using Constellation.Application.DTOs.CSV;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Gateways
{
    public class SchoolRegisterGateway : ISchoolRegisterGateway, IScopedService
    {
        private readonly HttpClient _client;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISchoolContactService _schoolContactService;

        public SchoolRegisterGateway(IUnitOfWork unitOfWork, ISchoolContactService schoolContactService)
        {
            var config = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };

            var proxy = WebRequest.DefaultWebProxy;
            config.UseProxy = true;
            config.Proxy = proxy;

            _client = new HttpClient(config);
            _unitOfWork = unitOfWork;
            _schoolContactService = schoolContactService;
        }

        public async Task GetSchoolPrincipals()
        {
            // Get csv file from https://datacollections.det.nsw.edu.au/listofschools/csv/listofschool_all.csv
            // Read entries into a collection of objects
            var csvSchools = await GetSchoolList();

            // Filter out all closed/proposed schools
            var openSchools = csvSchools.Where(school => school.Status == "Open" && !string.IsNullOrWhiteSpace(school.PrincipalEmail)).ToList();

            // Match entries with database
            var dbSchools = await _unitOfWork.Schools.ForBulkUpdate();
            var dbContacts = await _unitOfWork.SchoolContacts.ForBulkUpdate();

            foreach (var csvSchool in openSchools)
            {
                Console.WriteLine($"Processing School {csvSchool.Name} ({csvSchool.SchoolCode})");
                var dbSchool = dbSchools.SingleOrDefault(school => school.Code == csvSchool.SchoolCode);

                if (dbSchool == null)
                    continue;

                var principal = dbSchool.StaffAssignments.FirstOrDefault(role => role.Role == SchoolContactRole.Principal && !role.IsDeleted);

                if (string.IsNullOrWhiteSpace(csvSchool.PrincipalEmail))
                {
                    await _unitOfWork.CompleteAsync();
                    continue;
                }

                // Compare Principal in database to information in csv by email address
                // If different, mark database entry as old/deleted and create a new entry
                if (principal != null)
                {
                    if (!dbSchool.Students.Any(student => !student.IsDeleted) || !dbSchool.Staff.Any(staff => !staff.IsDeleted))
                    {
                        Console.WriteLine($" Removing old Principal: {principal.SchoolContact.DisplayName}");
                        await _schoolContactService.RemoveRole(principal.Id);

                        await _unitOfWork.CompleteAsync();
                        continue;
                    } else if (principal.SchoolContact.EmailAddress.ToLower() != csvSchool.PrincipalEmail.ToLower())
                    {
                        Console.WriteLine($" Removing old Principal: {principal.SchoolContact.DisplayName}");
                        await _schoolContactService.RemoveRole(principal.Id);
                        principal = null;
                    }
                }

                if (principal == null)
                {
                    // Does the email address appear in the SchoolContact list?
                    //var contact = await _unitOfWork.SchoolContacts.FromEmailForExistCheck(csvSchool.PrincipalEmail);
                    var contact = dbContacts.FirstOrDefault(contact => contact.EmailAddress.ToLower() == csvSchool.PrincipalEmail.ToLower());
                    if (contact == null)
                    {
                        Console.WriteLine($" Adding new Principal: {csvSchool.PrincipalEmail}");
                        var contactResult = await _schoolContactService.CreateContact(new SchoolContactDto
                        {
                            FirstName = csvSchool.PrincipalFirstName,
                            LastName = csvSchool.PrincipalLastName,
                            EmailAddress = csvSchool.PrincipalEmail
                        });

                        contact = contactResult.Entity;
                    }

                    Console.WriteLine($" Linking Principal {contact.DisplayName} with {csvSchool.Name}");
                    await _schoolContactService.CreateRole(new SchoolContactRoleDto
                    {
                        Role = SchoolContactRole.Principal,
                        SchoolCode = csvSchool.SchoolCode,
                        SchoolContactId = contact.Id
                    });
                }

                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<ICollection<CSVSchool>> GetSchoolList()
        {
            var response = await _client.GetAsync("https://datacollections.det.nsw.edu.au/listofschools/csv/listofschool_all.csv");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var entries = content.Split('\u000A').ToList();

            var textInfo = new CultureInfo("en-AU", false).TextInfo;

            var list = new List<CSVSchool>();
            var CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            foreach (var entry in entries)
            {
                var splitString = CSVParser.Split(entry);
                if (splitString[0].Length > 4 || splitString.Length == 1)
                    continue;

                try
                {
                    var school = new CSVSchool
                    {
                        SchoolCode = splitString[0].Trim(),
                        Name = splitString[1].Trim('"').Trim(),
                        Address = splitString[3].Trim(),
                        Town = textInfo.ToTitleCase(splitString[4].Trim()),
                        PostCode = splitString[5].Trim(),
                        Status = splitString[9].Trim(),
                        Electorate = splitString[17].Trim(),
                        PrincipalNetwork = splitString[24].Trim(),
                        Division = splitString[21].Trim(),
                        PhoneNumber = Regex.Replace(splitString[43].Trim(), @"[^0-9]", ""),
                        EmailAddress = splitString[46].Trim(),
                        FaxNumber = Regex.Replace(splitString[47].Trim(), @"[^0-9]", ""),
                        HeatSchool = splitString[50] == "Yes",
                        PrincipalName = splitString[73].Trim(),
                        PrincipalEmail = splitString[74].Trim()
                    };

                    if (school.PhoneNumber.Length == 8)
                        school.PhoneNumber = $"02{school.PhoneNumber}";

                    if (school.FaxNumber.Length == 8)
                        school.FaxNumber = $"02{school.FaxNumber}";

                    if (school.PrincipalName.IndexOf(',') > 0)
                    {
                        school.PrincipalName = school.PrincipalName.Trim('"').Trim();
                        var principalName = school.PrincipalName.Split(',');
                        school.PrincipalName = $"{principalName[1].Trim()} {principalName[0].Trim()}";
                        school.PrincipalFirstName = principalName[1].Trim();
                        school.PrincipalLastName = principalName[0].Trim();
                    }

                    list.Add(school);
                }
                catch (Exception)
                {

                    throw;
                }
            }

            return list;
        }
    }
}
