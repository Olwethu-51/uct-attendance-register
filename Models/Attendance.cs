namespace UCTAttendanceRegister.Models;

public enum AttendanceStatus
{
    Absent = 0,
    Present = 1
}

public class Attendance
{
    public int AttendanceId { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int LectureId { get; set; }
    public Lecture Lecture { get; set; } = null!;

    public AttendanceStatus Status { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AttendanceQuery> Queries { get; set; } = new List<AttendanceQuery>();
}