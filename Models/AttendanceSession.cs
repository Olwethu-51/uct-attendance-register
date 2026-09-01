using System.ComponentModel.DataAnnotations;

namespace UCTAttendanceRegister.Models;

public class AttendanceSession
{
    public int AttendanceSessionId { get; set; }

    public int LectureId { get; set; }
    public Lecture Lecture { get; set; } = null!;

    [MaxLength(10)]
    public string SessionCode { get; set; } = string.Empty;

    [MaxLength(256)]
    public string QrToken { get; set; } = string.Empty;

    public DateTime OpenedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;
}