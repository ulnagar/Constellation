namespace Constellation.Application.DTOs;

using Core.Models.Students.Identifiers;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.ValueObjects;
using System;
using System.Collections.Generic;

public abstract class TimetableDataDto
{
    public ICollection<TimetableData> Timetables { get; set; } = new List<TimetableData>();

    public class TimetableData
    {
        public Timetable Timetable { get; set; }
        public PeriodWeek Week { get; set; }
        public PeriodDay Day { get; set; }
        public char PeriodCode { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeOnly EntryTime { get; set; } = TimeOnly.MinValue;
        public TimeSpan EndTime { get; set; }
        public TimeOnly ExitTime { get; set; } = TimeOnly.MinValue;
        public string Name { get; set; }
        public PeriodType Type { get; set; }
        public string ClassName { get; set; }
        public string ClassTeacher { get; set; }
    }
}

public class StudentTimetableDataDto : TimetableDataDto
{
    public StudentId StudentId { get; set; }
    public string StudentName { get; set; }
    public string StudentSchool { get; set; }
    public string StudentGrade { get; set; }
    public bool HasAttendancePlan { get; set; } = false;
}

public class ClassTimetableDataDto : TimetableDataDto
{
    public string ClassName { get; set; }
}