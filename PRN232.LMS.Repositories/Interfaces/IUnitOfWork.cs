using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : class;

    IGenericRepository<Semester> Semesters { get; }
    IGenericRepository<Course> Courses { get; }
    IGenericRepository<Subject> Subjects { get; }
    IGenericRepository<Student> Students { get; }
    IGenericRepository<Enrollment> Enrollments { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
