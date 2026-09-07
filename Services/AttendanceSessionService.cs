using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using UCTAttendanceRegister.Data;
using UCTAttendanceRegister.Models;

namespace UCTAttendanceRegister.Services;

public class AttendanceSessionService
{
    private readonly ApplicationDbContext _context;

    // Excludes visually ambiguous characters (0/O, 1/I/L) so students
    // can type a session code accurately from a projector screen.
    private const string CodeCharset = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private const int SessionValidityMinutes = 15;

    public AttendanceSessionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceSession> OpenSessionAsync(int lectureId)
    {
        // Deactivate any previously open session for this lecture first —
        // only one session should be valid at a time per lecture.
        var existingActive = await _context.AttendanceSessions
            .Where(s => s.LectureId == lectureId && s.IsActive)
            .ToListAsync();

        foreach (var old in existingActive)
        {
            old.IsActive = false;
        }

        var now = DateTime.UtcNow;

        var session = new AttendanceSession
        {
            LectureId = lectureId,
            SessionCode = GenerateSessionCode(),
            QrToken = GenerateQrToken(),
            OpenedAt = now,
            ExpiresAt = now.AddMinutes(SessionValidityMinutes),
            IsActive = true
        };

        _context.AttendanceSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task<AttendanceSession?> GetActiveSessionForLectureAsync(int lectureId)
    {
        var session = await _context.AttendanceSessions
            .Where(s => s.LectureId == lectureId && s.IsActive)
            .OrderByDescending(s => s.OpenedAt)
            .FirstOrDefaultAsync();

        return IsUsable(session) ? session : null;
    }

    public async Task<AttendanceSession?> ValidateSessionCodeAsync(string code)
    {
        var session = await _context.AttendanceSessions
            .FirstOrDefaultAsync(s => s.SessionCode == code.ToUpperInvariant());

        return IsUsable(session) ? session : null;
    }

    public async Task<AttendanceSession?> ValidateQrTokenAsync(string token)
    {
        var session = await _context.AttendanceSessions
            .FirstOrDefaultAsync(s => s.QrToken == token);

        return IsUsable(session) ? session : null;
    }

    // The single source of truth for "is this session still valid right now" —
    // both the code path and the QR path funnel through here, so the
    // 15-minute rule can never be checked inconsistently between them.
    private static bool IsUsable(AttendanceSession? session)
    {
        return session is { IsActive: true } && DateTime.UtcNow <= session.ExpiresAt;
    }

    private static string GenerateSessionCode()
    {
        Span<char> chars = stackalloc char[6];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = CodeCharset[RandomNumberGenerator.GetInt32(CodeCharset.Length)];
        }
        return new string(chars);
    }

    private static string GenerateQrToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
    }
}