using System.ComponentModel.DataAnnotations;

namespace UCTAttendanceRegister.Models;

public class Lecturer
{
    public int LecturerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}