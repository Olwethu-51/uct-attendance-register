using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UCTAttendanceRegister.Data;
using UCTAttendanceRegister.Models;

namespace UCTAttendanceRegister.Pages.Lecturer;

[Authorize(Roles = "Lecturer")]
public class LecturesModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LecturesModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> CourseOptions { get; set; } = new();
    public List<Lecture> Lectures { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        [DataType(DataType.DateTime)]
        public DateTime? StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        [DataType(DataType.DateTime)]
        public DateTime? EndTime { get; set; }

        [Required]
        [MaxLength(100)]
        public string Venue { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.EndTime <= Input.StartTime)
        {
            ModelState.AddModelError("Input.EndTime", "End time must be after the start time.");
        }

        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        var userId = _userManager.GetUserId(User);
        var lecturer = await _context.Lecturers
            .FirstOrDefaultAsync(l => l.ApplicationUserId == userId);

        if (lecturer == null)
        {
            return NotFound();
        }

        // Confirm the selected course actually belongs to this lecturer —
        // otherwise a manipulated CourseId in the posted form could let a
        // lecturer schedule a lecture under someone else's course.
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseId == Input.CourseId && c.LecturerId == lecturer.LecturerId);

        if (course == null)
        {
            ModelState.AddModelError("Input.CourseId", "Invalid course selection.");
            await LoadDataAsync();
            return Page();
        }

        _context.Lectures.Add(new Lecture
        {
            CourseId = course.CourseId,
            StartTime = Input.StartTime!.Value,
            EndTime = Input.EndTime!.Value,
            Venue = Input.Venue
        });

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    private async Task LoadDataAsync()
    {
        var userId = _userManager.GetUserId(User);
        var lecturer = await _context.Lecturers
            .FirstOrDefaultAsync(l => l.ApplicationUserId == userId);

        if (lecturer == null) return;

        var courses = await _context.Courses
            .Where(c => c.LecturerId == lecturer.LecturerId)
            .ToListAsync();

        CourseOptions = courses
            .Select(c => new SelectListItem($"{c.CourseCode} - {c.CourseName}", c.CourseId.ToString()))
            .ToList();

        Lectures = await _context.Lectures
            .Where(l => l.Course.LecturerId == lecturer.LecturerId)
            .Include(l => l.Course)
            .OrderByDescending(l => l.StartTime)
            .ToListAsync();
    }
}