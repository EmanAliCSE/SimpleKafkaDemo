
 
using System.Linq.Expressions;


namespace Infrastructure.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        public bool UpdateRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        Task<T> GetByIdAsync(object id);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);
        Task<T> FirstOrDefaultAsync();
        Task<IEnumerable<T>> ListAsync();
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges=false);
    }
}
