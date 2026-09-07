using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UCTAttendanceRegister.Data;
using UCTAttendanceRegister.Models;
using UCTAttendanceRegister.Services;

namespace UCTAttendanceRegister.Pages.Student;

[Authorize(Roles = "Student")]
public class CheckInModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AttendanceSessionService _sessionService;

    public CheckInModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        AttendanceSessionService sessionService)
    {
        _context = context;
        _userManager = userManager;
        _sessionService = sessionService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? StatusMessage { get; set; }
    public bool CheckedInSuccessfully { get; set; }

    public class InputModel
    {
        [Display(Name = "Session Code")]
        public string? SessionCode { get; set; }
    }

    // GET is hit two ways: a bare visit to the page (manual code entry),
    // or a scanned QR link that includes ?token=... — in the QR case we
    // attempt check-in immediately rather than making the student retype anything.
    public async Task<IActionResult> OnGetAsync(string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            var session = await _sessionService.ValidateQrTokenAsync(token);
            await TryRecordAttendanceAsync(session);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.SessionCode))
        {
            StatusMessage = "Please enter a session code.";
            return Page();
        }

        var session = await _sessionService.ValidateSessionCodeAsync(Input.SessionCode.Trim());
        await TryRecordAttendanceAsync(session);

        return Page();
    }

    private async Task TryRecordAttendanceAsync(AttendanceSession? session)
{
    if (session == null)
    {
        StatusMessage = "This session code or QR code is invalid or has expired.";
        return;
    }

    var userId = _userManager.GetUserId(User);
    var student = await _context.Students
        .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

    if (student == null)
    {
        StatusMessage = "Student record not found.";
        return;
    }

    var lecture = await _context.Lectures
        .FirstOrDefaultAsync(l => l.LectureId == session.LectureId);

    if (lecture == null)
    {
        StatusMessage = "Lecture not found.";
        return;
    }

    var isEnrolled = await _context.CourseEnrollments
        .AnyAsync(ce => ce.StudentId == student.StudentId && ce.CourseId == lecture.CourseId);

    if (!isEnrolled)
    {
        StatusMessage = "You are not enrolled in this course, so attendance cannot be recorded.";
        return;
    }

    // Prevent a duplicate Attendance row if the student scans/submits twice —
    // the unique index on (StudentId, LectureId) would also catch this at the
    // DB level, but checking here lets us show a friendly message instead of
    // a raw SQL exception.
    var existing = await _context.Attendances
        .FirstOrDefaultAsync(a => a.StudentId == student.StudentId && a.LectureId == session.LectureId);

    if (existing != null)
    {
        StatusMessage = "You have already checked in for this lecture.";
        CheckedInSuccessfully = true;
        return;
    }

    _context.Attendances.Add(new Attendance
    {
        StudentId = student.StudentId,
        LectureId = session.LectureId,
        Status = AttendanceStatus.Present,
        RecordedAt = DateTime.UtcNow
    });

    await _context.SaveChangesAsync();

    StatusMessage = "You have been marked present. Well done!";
    CheckedInSuccessfully = true;
}}