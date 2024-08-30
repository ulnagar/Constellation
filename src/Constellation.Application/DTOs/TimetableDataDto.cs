namespace Constellation.Application.DTOs;

using Core.Models.Students.Identifiers;
using System;
using System.Collections.Generic;

public abstract class TimetableDataDto
{
    public ICollection<TimetableData> Timetables { get; set; } = new List<TimetableData>();

    public class TimetableData
    {
        public string TimetableName { get; set; }
        public int Day { get; set; }
        public int Period { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
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
}

public class ClassTimetableDataDto : TimetableDataDto
{
    public string ClassName { get; set; }
}