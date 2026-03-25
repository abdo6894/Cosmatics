using Cosmatics.Infrastructure.Exceptions;
using Cosmatics.Infrastructure.Persistense.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Cosmatics.Infrastructure.Persistense.Repository_Pattern;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;
    private readonly ILogger<Repository<T>> _log;
    public Repository(AppDbContext context, ILogger<Repository<T>> log)
    {
        _context = context;
        _dbSet = _context.Set<T>();
        _log = log;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _dbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new DataAccessException(ex,"An error occurred while retrieving data.", _log);
        }
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
    
        try
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new DataAccessException(ex, "An error occurred while retrieving data.", _log);
        }
    }


    public async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new DataAccessException(ex, "An error occurred while retrieving data.", _log);
        }
    }

    public async Task AddAsync(T entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
        }
        catch
        (Exception ex)
        {
            throw new DataAccessException(ex, "An error occurred while adding data.", _log);
        }
    }

    public async Task UpdateAsync(T entity)
    {
        try
        {
            var entry = _context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
                entry.State = EntityState.Modified;
            }
         
            await SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new DataAccessException(ex, "An error occurred while updating data.", _log);
        }
    }
    public async Task DeleteAsync(T entity)
    {

        try
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
            await SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new DataAccessException(ex, "An error occurred while deleting data.", _log);
        }
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
