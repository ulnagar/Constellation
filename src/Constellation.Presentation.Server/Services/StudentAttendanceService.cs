namespace Constellation.Presentation.Server.Services;

using Application.Attendance.GetAttendanceDataFromSentral;

public sealed class StudentAttendanceService
{
    private readonly List<StudentAttendanceData> _attendanceData = new();

    public StudentAttendanceService()
    {
        
    }

    public void AddItem(StudentAttendanceData attendanceItem) => _attendanceData.Add(attendanceItem);

    public void AddItems(List<StudentAttendanceData> attendanceItems) => _attendanceData.AddRange(attendanceItems);

    public List<StudentAttendanceData> GetAllData => _attendanceData.ToList();

    public List<StudentAttendanceData> GetDataForStudent(string studentId) => _attendanceData.Where(entry => entry.StudentId == studentId).ToList();

    public List<string> GetStudents => _attendanceData.Select(entry => entry.StudentId).Distinct().ToList();

}
