using System.ComponentModel.DataAnnotations;

namespace UCTAttendanceRegister.Models;

public class Student
{
    public int StudentId { get; set; }

    [Required]
    [MaxLength(20)]
    public string StudentNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();
    public ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
    public ICollection<AttendanceQuery> AttendanceQueries { get; set; } = new List<AttendanceQuery>();
}