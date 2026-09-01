namespace UCTAttendanceRegister.Models;

public class CourseEnrollment
{
    // Composite primary key (StudentId, CourseId) — configured in ApplicationDbContext,
    // since composite keys can't be expressed with data annotations alone.
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}