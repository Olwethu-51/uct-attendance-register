using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UCTAttendanceRegister.Data;
using UCTAttendanceRegister.Models;

namespace UCTAttendanceRegister.Pages.Lecturer;

[Authorize(Roles = "Lecturer")]
public class CoursesModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CoursesModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Course> Courses { get; set; } = new();

    public class InputModel
    {
        [Required]
        [MaxLength(20)]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        [Range(0, 100)]
        [Display(Name = "Minimum Attendance %")]
        public decimal MinimumAttendancePercentage { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadCoursesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync();
            return Page();
        }

        var userId = _userManager.GetUserId(User);
        var lecturer = await _context.Lecturers
            .FirstOrDefaultAsync(l => l.ApplicationUserId == userId);

        if (lecturer == null)
        {
            return NotFound();
        }

        // Course codes must be unique (enforced by the DB too), so check
        // here first to give a friendly error instead of a raw SQL exception.
        var codeExists = await _context.Courses
            .AnyAsync(c => c.CourseCode == Input.CourseCode);

        if (codeExists)
        {
            ModelState.AddModelError("Input.CourseCode", "A course with this code already exists.");
            await LoadCoursesAsync();
            return Page();
        }

        _context.Courses.Add(new Course
        {
            CourseCode = Input.CourseCode,
            CourseName = Input.CourseName,
            MinimumAttendancePercentage = Input.MinimumAttendancePercentage,
            LecturerId = lecturer.LecturerId
        });

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    private async Task LoadCoursesAsync()
    {
        var userId = _userManager.GetUserId(User);
        var lecturer = await _context.Lecturers
            .FirstOrDefaultAsync(l => l.ApplicationUserId == userId);

        if (lecturer != null)
        {
            Courses = await _context.Courses
                .Where(c => c.LecturerId == lecturer.LecturerId)
                .ToListAsync();
        }
    }
}