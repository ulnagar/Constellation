using Constellation.Application.DTOs;
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

                // Compare Principal in database to information in csv by email address
                // If different, mark database entry as old/deleted and create a new entry
                if (principal != null && principal.SchoolContact.EmailAddress.ToLower() != csvSchool.PrincipalEmail.ToLower())
                {
                    Console.WriteLine($" Removing old Principal: {principal.SchoolContact.DisplayName}");
                    await _schoolContactService.RemoveRole(principal.Id);
                    principal = null;
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
                        SchoolContact = contact
                    });
                }

                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task UpdateSchoolDetails()
        {
            // Get csv file from https://datacollections.det.nsw.edu.au/listofschools/csv/listofschool_all.csv
            // Read entries into a collection of objects
            var csvSchools = await GetSchoolList();

            // Filter out all closed/proposed schools
            var openSchools = csvSchools.Where(school => school.Status == "Open").ToList();

            // Match entries with database
            var dbSchools = await _unitOfWork.Schools.ForBulkUpdate();

            foreach (var csvSchool in openSchools)
            {
                Console.WriteLine($"Processing School {csvSchool.Name} ({csvSchool.SchoolCode})");
                var dbSchool = dbSchools.SingleOrDefault(school => school.Code == csvSchool.SchoolCode);

                if (dbSchool == null)
                {
                    Console.WriteLine($" New school - Adding to database");
                    // Doesn't exist in database! Create!
                    var newSchool = new School
                    {
                        Code = csvSchool.SchoolCode,
                        Name = csvSchool.Name,
                        Address = csvSchool.Address,
                        Town = csvSchool.Town,
                        State = "NSW",
                        PostCode = csvSchool.PostCode,
                        PhoneNumber = csvSchool.PhoneNumber,
                        FaxNumber = csvSchool.FaxNumber,
                        EmailAddress = csvSchool.EmailAddress,
                        Division = csvSchool.Division,
                        HeatSchool = csvSchool.HeatSchool,
                        Electorate = csvSchool.Electorate,
                        PrincipalNetwork = csvSchool.PrincipalNetwork
                    };

                    _unitOfWork.Add(newSchool);
                } else
                {
                    // Update database entry if required
                    if (dbSchool.Name != csvSchool.Name)
                    {
                        LogChange("Name", dbSchool.Name, csvSchool.Name);
                        dbSchool.Name = csvSchool.Name;
                    }

                    if (dbSchool.Address != csvSchool.Address)
                    {
                        LogChange("Address", dbSchool.Address, csvSchool.Address);    
                        dbSchool.Address = csvSchool.Address;
                    }

                    if (dbSchool.Town != csvSchool.Town)
                    {
                        LogChange("Town", dbSchool.Town, csvSchool.Town);
                        dbSchool.Town = csvSchool.Town;
                    }

                    if (dbSchool.PostCode != csvSchool.PostCode)
                    {
                        LogChange("PostCode", dbSchool.PostCode, csvSchool.PostCode);
                        dbSchool.PostCode = csvSchool.PostCode;
                    }

                    if (dbSchool.Electorate != csvSchool.Electorate)
                    {
                        LogChange("Electorate", dbSchool.Electorate, csvSchool.Electorate);
                        dbSchool.Electorate = csvSchool.Electorate;
                    }

                    if (dbSchool.PrincipalNetwork != csvSchool.PrincipalNetwork)
                    {
                        LogChange("PrincipalNetwork", dbSchool.PrincipalNetwork, csvSchool.PrincipalNetwork);
                        dbSchool.PrincipalNetwork = csvSchool.PrincipalNetwork;
                    }

                    if (dbSchool.Division != csvSchool.Division)
                    {
                        LogChange("Division", dbSchool.Division, csvSchool.Division);
                        dbSchool.Division = csvSchool.Division;
                    }

                    if (dbSchool.PhoneNumber != csvSchool.PhoneNumber)
                    {
                        LogChange("PhoneNumber", dbSchool.PhoneNumber, csvSchool.PhoneNumber);
                        dbSchool.PhoneNumber = csvSchool.PhoneNumber;
                    }

                    if (dbSchool.EmailAddress != csvSchool.EmailAddress)
                    {
                        LogChange("EmailAddress", dbSchool.EmailAddress, csvSchool.EmailAddress);
                        dbSchool.EmailAddress = csvSchool.EmailAddress;
                    }

                    if (dbSchool.FaxNumber != csvSchool.FaxNumber)
                    {
                        LogChange("FaxNumber", dbSchool.FaxNumber, csvSchool.FaxNumber);
                        dbSchool.FaxNumber = csvSchool.FaxNumber;
                    }

                    if (dbSchool.HeatSchool != csvSchool.HeatSchool)
                    {
                        LogChange("HeatSchool", dbSchool.HeatSchool.ToString(), csvSchool.HeatSchool.ToString());
                        dbSchool.HeatSchool = csvSchool.HeatSchool;
                    }
                }

                await _unitOfWork.CompleteAsync();
            }
        }

        private void LogChange(string property, string oldValue, string newValue)
        {
            Console.WriteLine($" Updating {property} from {oldValue} to {newValue}");
        }

        private async Task<ICollection<CSVSchool>> GetSchoolList()
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

        private class CSVSchool
        {
            //Column 0
            public string SchoolCode { get; set; }
            //Column 1
            public string Name { get; set; }
            //Column 3
            public string Address { get; set; }
            //Column 4
            public string Town { get; set; }
            //Column 5
            public string PostCode { get; set; }
            //Column 9
            public string Status { get; set; }
            //Column 17
            public string Electorate { get; set; }
            //Column 24
            public string PrincipalNetwork { get; set; }
            //Column 21
            public string Division { get; set; }
            //Column 43
            public string PhoneNumber { get; set; }
            //Column 46
            public string EmailAddress { get; set; }
            //Column 47
            public string FaxNumber { get; set; }
            //Column 50
            public bool HeatSchool { get; set; }
            //Column 73
            public string PrincipalName { get; set; }
            public string PrincipalFirstName { get; set; }
            public string PrincipalLastName { get; set; }
            //Column 74
            public string PrincipalEmail { get; set; }
        }
    }
}
