namespace Constellation.Presentation.Server.Services;

using Core.Models.Attendance;

public sealed class StudentAttendanceService
{
    private readonly List<AttendanceValue> _attendanceData = new();


    public void AddItem(AttendanceValue attendanceItem) => _attendanceData.Add(attendanceItem);

    public void AddItems(List<AttendanceValue> attendanceItems) => _attendanceData.AddRange(attendanceItems);

    public List<AttendanceValue> GetAllData => _attendanceData.ToList();

    public List<AttendanceValue> GetDataForStudent(string studentId) => _attendanceData.Where(entry => entry.StudentId == studentId).ToList();

    public List<string> GetStudents => _attendanceData.Select(entry => entry.StudentId).Distinct().ToList();

}
