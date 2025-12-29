using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace DigitalHub.Services.Shared
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();
        IList<TEntity> GetAllMatched(Expression<Func<TEntity, bool>> match);
        IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] includeProperties);
        TEntity GetById(object id);
        TEntity Find(Expression<Func<TEntity, bool>> match);
        IQueryable<TEntity> GetIQueryable();
        IList<TEntity> GetAllPaged(int pageIndex, int pageSize, out int totalCount);
        int Count();
        EntityEntry<TEntity> Insert(TEntity entity, bool saveChanges = false);
        void Delete(object id, bool saveChanges = false);
        void Delete(TEntity entity, bool saveChanges = false);
        void Update(TEntity entity, bool saveChanges = false);
        TEntity Update(TEntity entity, object key, bool saveChanges = false);
        void Commit();

        Task<IList<TEntity>> GetAllAsync();
        Task<IList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> match);
        Task<TEntity> GetByIdAsync(object id);
        Task<TEntity> FindAsyncNoTracking(Expression<Func<TEntity, bool>> match);
        Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> match);
        Task<int> CountAsync();
        Task<TEntity> InsertAsync(TEntity entity, bool saveChanges = false);
        Task DeleteAsync(object id, bool saveChanges = false);
        Task DeleteAsync(TEntity entity, bool saveChanges = false);
        Task UpdateAsync(TEntity entity, bool saveChanges = false);
        Task<TEntity> UpdateAsync(TEntity entity, object key, bool saveChanges = false);
        Task CommitAsync();
        void Dispose();
        IQueryable<TEntity> GetAllIncludingNoTracking(params Expression<Func<TEntity, object>>[] includeProperties);
        void InsertAll(List<TEntity> entity, bool saveChanges = false);
        void UpdateAll(IEnumerable<TEntity> entities, bool saveChanges = false);
    }
}