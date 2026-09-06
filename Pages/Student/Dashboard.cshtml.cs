using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UCTAttendanceRegister.Data;
using UCTAttendanceRegister.Models;
using Microsoft.AspNetCore.Mvc;

namespace UCTAttendanceRegister.Pages.Student;

[Authorize(Roles = "Student")]
public class DashboardModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public UCTAttendanceRegister.Models.Student? CurrentStudent { get; set; }
    public List<Course> EnrolledCourses { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);

        CurrentStudent = await _context.Students
            .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

        if (CurrentStudent == null)
        {
            // Student record missing despite being in the Student role —
            // shouldn't normally happen given how Register.cshtml.cs works,
            // but guard against it rather than crashing.
            return NotFound();
        }

        EnrolledCourses = await _context.CourseEnrollments
            .Where(ce => ce.StudentId == CurrentStudent.StudentId)
            .Include(ce => ce.Course)
            .Select(ce => ce.Course)
            .ToListAsync();

        return Page();
    }
}