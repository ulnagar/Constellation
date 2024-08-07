﻿namespace Constellation.Core.Models;

using Faculties;
using Shared;
using System;
using System.Collections.Generic;
using Training;
using ValueObjects;

public class Staff
{
    public string StaffId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PortalUsername { get; set; }
    public string AdobeConnectPrincipalId { get; set; }
    public virtual List<FacultyMembership> Faculties { get; set; } = new();
    public bool IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public DateTime? DateEntered { get; set; } = DateTime.Now;
    public string SchoolCode { get; set; }
    public virtual School School { get; set; }
    public string DisplayName => FirstName + " " + LastName;
    public string EmailAddress => PortalUsername + "@det.nsw.edu.au";
    public bool IsShared { get; set; }
    public virtual List<TeacherMSTeamOperation> MSTeamOperations { get; set; } = new();
    public virtual List<TrainingCompletion> TrainingCompletionRecords { get; set; } = new();

    public Name? GetName()
    {
        Result<Name> request = Name.Create(FirstName, string.Empty, LastName);

        if (request.IsSuccess)
            return request.Value;

        return null;
    }
}