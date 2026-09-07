using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UCTAttendanceRegister.Data;
using UCTAttendanceRegister.Models;

namespace UCTAttendanceRegister.Pages.Student;

[Authorize(Roles = "Student")]
public class EnrollModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EnrollModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<Course> AvailableCourses { get; set; } = new();
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAvailableCoursesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int courseId)
    {
        var userId = _userManager.GetUserId(User);
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

        if (student == null) return NotFound();

        var alreadyEnrolled = await _context.CourseEnrollments
            .AnyAsync(ce => ce.StudentId == student.StudentId && ce.CourseId == courseId);

        if (!alreadyEnrolled)
        {
            _context.CourseEnrollments.Add(new CourseEnrollment
            {
                StudentId = student.StudentId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        StatusMessage = "Enrolled successfully.";
        await LoadAvailableCoursesAsync();
        return Page();
    }

    private async Task LoadAvailableCoursesAsync()
    {
        var userId = _userManager.GetUserId(User);
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

        if (student == null) return;

        var enrolledCourseIds = await _context.CourseEnrollments
            .Where(ce => ce.StudentId == student.StudentId)
            .Select(ce => ce.CourseId)
            .ToListAsync();

        AvailableCourses = await _context.Courses
            .Where(c => !enrolledCourseIds.Contains(c.CourseId))
            .ToListAsync();
    }
}