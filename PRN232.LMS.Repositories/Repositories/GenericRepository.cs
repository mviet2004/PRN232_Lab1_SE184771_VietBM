using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Context;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly LmsDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(LmsDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(object id) =>
        await _dbSet.FindAsync(id);

    public async Task<T?> GetByIdAsync(object id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(BuildIdPredicate(id));
    }

    public IQueryable<T> GetAll() => _dbSet.AsNoTracking();

    public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate) =>
        _dbSet.AsNoTracking().Where(predicate);

    public IQueryable<T> GetAll(Expression<Func<T, bool>>? predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return query;
    }

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);

    public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.AnyAsync(predicate);

    private static Expression<Func<T, bool>> BuildIdPredicate(object id)
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = typeof(T).GetProperties()
            .FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.Ordinal));

        if (property is null)
        {
            throw new InvalidOperationException($"Entity {typeof(T).Name} does not have an Id property.");
        }

        var left = Expression.Property(parameter, property);
        var right = Expression.Constant(Convert.ChangeType(id, property.PropertyType));
        var body = Expression.Equal(left, right);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
