using Microsoft.EntityFrameworkCore.Storage;
using PRN232.LMS.Repositories.Context;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly LmsDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    public UnitOfWork(LmsDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        if (!_repositories.TryGetValue(type, out var repository))
        {
            repository = new GenericRepository<T>(_context);
            _repositories[type] = repository;
        }

        return (IGenericRepository<T>)repository;
    }

    public IGenericRepository<Semester> Semesters => Repository<Semester>();
    public IGenericRepository<Course> Courses => Repository<Course>();
    public IGenericRepository<Subject> Subjects => Repository<Subject>();
    public IGenericRepository<Student> Students => Repository<Student>();
    public IGenericRepository<Enrollment> Enrollments => Repository<Enrollment>();

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task BeginTransactionAsync() =>
        _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
