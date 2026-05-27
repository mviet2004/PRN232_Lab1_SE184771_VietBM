using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Context;

public class LmsDbContext : DbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Semester>(entity =>
        {
            entity.ToTable("semester");
            entity.HasKey(e => e.SemesterId);
            entity.Property(e => e.SemesterId).HasColumnName("semesterid");
            entity.Property(e => e.SemesterName).HasColumnName("semestername").HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnName("startdate");
            entity.Property(e => e.EndDate).HasColumnName("enddate");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("course");
            entity.HasKey(e => e.CourseId);
            entity.Property(e => e.CourseId).HasColumnName("courseid");
            entity.Property(e => e.CourseName).HasColumnName("coursename").HasMaxLength(100);
            entity.Property(e => e.SemesterId).HasColumnName("semesterid");
            entity.HasOne(e => e.Semester)
                .WithMany(s => s.Courses)
                .HasForeignKey(e => e.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("subject");
            entity.HasKey(e => e.SubjectId);
            entity.Property(e => e.SubjectId).HasColumnName("subjectid");
            entity.Property(e => e.SubjectCode).HasColumnName("subjectcode").HasMaxLength(20);
            entity.Property(e => e.SubjectName).HasColumnName("subjectname").HasMaxLength(100);
            entity.Property(e => e.Credit).HasColumnName("credit");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("student");
            entity.HasKey(e => e.StudentId);
            entity.Property(e => e.StudentId).HasColumnName("studentid");
            entity.Property(e => e.FullName).HasColumnName("fullname").HasMaxLength(100);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.DateOfBirth).HasColumnName("dateofbirth");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("enrollment");
            entity.HasKey(e => e.EnrollmentId);
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollmentid");
            entity.Property(e => e.StudentId).HasColumnName("studentid");
            entity.Property(e => e.CourseId).HasColumnName("courseid");
            entity.Property(e => e.EnrollDate).HasColumnName("enrolldate");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
