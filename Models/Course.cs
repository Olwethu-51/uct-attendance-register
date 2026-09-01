using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCTAttendanceRegister.Models;

public class Course
{
    public int CourseId { get; set; }

    [Required]
    [MaxLength(20)]
    public string CourseCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string CourseName { get; set; } = string.Empty;

    // The lecturer-defined DP attendance requirement for THIS course, e.g. 80.00
    [Column(TypeName = "decimal(5,2)")]
    public decimal MinimumAttendancePercentage { get; set; }

    public int LecturerId { get; set; }
    public Lecturer Lecturer { get; set; } = null!;

    public ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();
    public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
}