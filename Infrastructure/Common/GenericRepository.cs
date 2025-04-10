
 
using System.Linq.Expressions;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _dbContext;

        public GenericRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(T entity)
        {
            _dbContext.Set<T>().Add(entity);
        }
        public void AddRange(IEnumerable<T> entities)
        {
            _dbContext.Set<T>().AddRange(entities);
        }
        public void Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);
        }
        public bool UpdateRange(IEnumerable<T> entities)
        {
            _dbContext.Set<T>().UpdateRange(entities);
            return true;
        }
        public void Remove(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }
        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbContext.Set<T>().RemoveRange(entities);
        }
        public async Task<T> GetByIdAsync(object id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbContext.Set<T>().FirstOrDefaultAsync(expression);
        }
        public async Task<T> FirstOrDefaultAsync()
        {
            return await _dbContext.Set<T>().FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<T>> ListAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }
       
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges=false)
        {

            if (trackChanges)
            {
                return _dbContext.Set<T>().Where(expression);
            }
            else
            {
                return _dbContext.Set<T>().Where(expression).AsNoTracking();
            }
        }
      
    }
}
