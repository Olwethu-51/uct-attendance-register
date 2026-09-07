using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using UCTAttendanceRegister.Data;
using UCTAttendanceRegister.Models;
using UCTAttendanceRegister.Services;

namespace UCTAttendanceRegister.Pages.Lecturer;

[Authorize(Roles = "Lecturer")]
public class OpenSessionModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AttendanceSessionService _sessionService;

    public OpenSessionModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        AttendanceSessionService sessionService)
    {
        _context = context;
        _userManager = userManager;
        _sessionService = sessionService;
    }

    public Lecture? SelectedLecture { get; set; }
    public AttendanceSession? Session { get; set; }
    public string? QrImageBase64 { get; set; }

    public async Task<IActionResult> OnGetAsync(int lectureId)
    {
        var userId = _userManager.GetUserId(User);
        var lecturer = await _context.Lecturers
            .FirstOrDefaultAsync(l => l.ApplicationUserId == userId);

        if (lecturer == null) return NotFound();

        // Ownership check — same reasoning as the Lectures page: never trust
        // a lectureId from the query string without confirming it's this
        // lecturer's own lecture.
        SelectedLecture = await _context.Lectures
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.LectureId == lectureId && l.Course.LecturerId == lecturer.LecturerId);

        if (SelectedLecture == null) return NotFound();

        Session = await _sessionService.GetActiveSessionForLectureAsync(lectureId);

        if (Session != null)
        {
            GenerateQrImage(Session.QrToken);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostOpenAsync(int lectureId)
    {
        var userId = _userManager.GetUserId(User);
        var lecturer = await _context.Lecturers
            .FirstOrDefaultAsync(l => l.ApplicationUserId == userId);

        if (lecturer == null) return NotFound();

        var lecture = await _context.Lectures
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.LectureId == lectureId && l.Course.LecturerId == lecturer.LecturerId);

        if (lecture == null) return NotFound();

        await _sessionService.OpenSessionAsync(lectureId);

        return RedirectToPage(new { lectureId });
    }

    private void GenerateQrImage(string token)
    {
        // The QR encodes a full check-in URL so a student's camera app
        // can jump straight to the check-in page — not just the bare token.
        var checkInUrl = Url.Page("/Student/CheckIn", null, new { token }, Request.Scheme)
    ?? throw new InvalidOperationException("Could not generate check-in URL.");

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(checkInUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var bytes = qrCode.GetGraphic(10);

        QrImageBase64 = Convert.ToBase64String(bytes);
    }
}