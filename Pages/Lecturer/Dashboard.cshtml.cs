using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UCTAttendanceRegister.Data;
using UCTAttendanceRegister.Models;

namespace UCTAttendanceRegister.Pages.Lecturer;

[Authorize(Roles = "Lecturer")]
public class DashboardModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public UCTAttendanceRegister.Models.Lecturer? CurrentLecturer { get; set; }
    public List<Course> Courses { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);

        CurrentLecturer = await _context.Lecturers
            .FirstOrDefaultAsync(l => l.ApplicationUserId == userId);

        if (CurrentLecturer == null)
        {
            return NotFound();
        }

        Courses = await _context.Courses
            .Where(c => c.LecturerId == CurrentLecturer.LecturerId)
            .ToListAsync();

        return Page();
    }
}