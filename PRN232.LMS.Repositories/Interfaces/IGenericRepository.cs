using System.Linq.Expressions;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<T?> GetByIdAsync(object id, params Expression<Func<T, object>>[] includes);
    IQueryable<T> GetAll();
    IQueryable<T> GetAll(Expression<Func<T, bool>> predicate);
    IQueryable<T> GetAll(Expression<Func<T, bool>>? predicate, params Expression<Func<T, object>>[] includes);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}
