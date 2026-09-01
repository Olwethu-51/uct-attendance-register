namespace UCTAttendanceRegister.Models;

public class Lecture
{
    public int LectureId { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public string Venue { get; set; } = string.Empty;

    public ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
    public ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();
}