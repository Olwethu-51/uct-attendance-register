using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UCTAttendanceRegister.Models;

namespace UCTAttendanceRegister.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Lecturer> Lecturers => Set<Lecturer>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
    public DbSet<Lecture> Lectures => Set<Lecture>();
    public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<AttendanceQuery> AttendanceQueries => Set<AttendanceQuery>();
    // Our domain DbSets will be added here
    // when we implement the entities.

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

 // Student <-> ApplicationUser (one-to-one)
        builder.Entity<Student>()
            .HasOne(s => s.ApplicationUser)
            .WithOne()
            .HasForeignKey<Student>(s => s.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Student>()
            .HasIndex(s => s.StudentNumber)
            .IsUnique();

        // Lecturer <-> ApplicationUser (one-to-one)
        builder.Entity<Lecturer>()
            .HasOne(l => l.ApplicationUser)
            .WithOne()
            .HasForeignKey<Lecturer>(l => l.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Course -> Lecturer
        builder.Entity<Course>()
            .HasOne(c => c.Lecturer)
            .WithMany(l => l.Courses)
            .HasForeignKey(c => c.LecturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Course>()
            .HasIndex(c => c.CourseCode)
            .IsUnique();

        // CourseEnrollment: composite key, many-to-many join table
        builder.Entity<CourseEnrollment>()
            .HasKey(ce => new { ce.StudentId, ce.CourseId });

        builder.Entity<CourseEnrollment>()
            .HasOne(ce => ce.Student)
            .WithMany(s => s.CourseEnrollments)
            .HasForeignKey(ce => ce.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CourseEnrollment>()
            .HasOne(ce => ce.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(ce => ce.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Lecture -> Course
        builder.Entity<Lecture>()
            .HasOne(l => l.Course)
            .WithMany(c => c.Lectures)
            .HasForeignKey(l => l.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // AttendanceSession -> Lecture
        builder.Entity<AttendanceSession>()
            .HasOne(a => a.Lecture)
            .WithMany(l => l.AttendanceSessions)
            .HasForeignKey(a => a.LectureId)
            .OnDelete(DeleteBehavior.Cascade);

        // Attendance -> Student & Lecture
        builder.Entity<Attendance>()
            .HasOne(a => a.Student)
            .WithMany(s => s.AttendanceRecords)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Attendance>()
            .HasOne(a => a.Lecture)
            .WithMany(l => l.AttendanceRecords)
            .HasForeignKey(a => a.LectureId)
            .OnDelete(DeleteBehavior.Restrict);

        // One attendance record per student per lecture
        builder.Entity<Attendance>()
            .HasIndex(a => new { a.StudentId, a.LectureId })
            .IsUnique();

        // AttendanceQuery -> Attendance & Student
        builder.Entity<AttendanceQuery>()
            .HasOne(q => q.Attendance)
            .WithMany(a => a.Queries)
            .HasForeignKey(q => q.AttendanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AttendanceQuery>()
            .HasOne(q => q.Student)
            .WithMany(s => s.AttendanceQueries)
            .HasForeignKey(q => q.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        // Entity relationships and constraints
        // will be configured here.
    }
}