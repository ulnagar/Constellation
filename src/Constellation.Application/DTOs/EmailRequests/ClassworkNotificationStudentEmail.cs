﻿namespace Constellation.Application.DTOs.EmailRequests;

using Constellation.Core.Models;
using System.Collections.Generic;

public class ClassworkNotificationStudentEmail
{
    public Absence Absence { get; set; }
    public ClassworkNotification Notification { get; set; }
    public List<string> ParentEmails { get; set; } = new();
}