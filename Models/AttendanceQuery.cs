using System.ComponentModel.DataAnnotations;

namespace UCTAttendanceRegister.Models;

public enum AttendanceQueryStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class AttendanceQuery
{
    public int AttendanceQueryId { get; set; }

    public int AttendanceId { get; set; }
    public Attendance Attendance { get; set; } = null!;

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;

    public AttendanceQueryStatus Status { get; set; } = AttendanceQueryStatus.Pending;

    [MaxLength(1000)]
    public string? LecturerResponse { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}