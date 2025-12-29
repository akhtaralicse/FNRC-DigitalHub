using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DigitalHub.Domain.DBContext;
using Microsoft.AspNetCore.Http;
using DigitalHub.Domain.Shared;
using DigitalHub.Domain.Domains;
using DigitalHub.Domain.Enums;
using Newtonsoft.Json; 

namespace DigitalHub.Services.Shared
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly DigitalHubDBContext Context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public GenericRepository(DigitalHubDBContext context, IHttpContextAccessor httpContextAccessor)
        {
            Context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public GenericRepository(DigitalHubDBContext context)
        {
            Context = context;
        }


        private void AddTimestamps()
        {
            var utcNow = DateTime.Now;
            var entities = Context.ChangeTracker.Entries().Where(x => x.Entity is BaseDomainEntity && (x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted));

            var userName = _httpContextAccessor.HttpContext.User.Identity.Name ?? "Anonymous";
          //  var userName = "";//AppHttpContext.UserClaims() != null ? AppHttpContext.UserClaims().Name : "job";
            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((BaseDomainEntity)entity.Entity).CreatedDate = utcNow;
                    ((BaseDomainEntity)entity.Entity).CreatedBy = userName;
                }
                else if (entity.State == EntityState.Deleted)
                {
                    UpdateLogs(new LogsLkp
                    {
                        TableName = entity.Metadata.GetTableName(),
                        TableId = (int?)(entity.OriginalValues[entity.Metadata.FindPrimaryKey().Properties.First()] ?? 0),
                        Action = SetValueChanges(entity), //JsonConvert.SerializeObject(entity)  ,
                        ActionType =  LogActionTypeEnum.Delete,
                        CreatedBy = userName,
                        CreatedDate = utcNow,
                    });
                }
                else if (entity.State == EntityState.Modified)
                {
                    ((BaseDomainEntity)entity.Entity).UpdatedDate = utcNow;
                    ((BaseDomainEntity)entity.Entity).UpdatedBy = userName;
                }
            }
        }
        private string SetValueChanges(EntityEntry Entry)
        {
            var OldValues = new Dictionary<string, object>();
            foreach (PropertyEntry property in Entry.Properties)
            {

                string propertyName = property.Metadata.Name;
                switch (Entry.State)
                {
                    //case EntityState.Added:
                    //    NewValues[propertyName] = property.CurrentValue;
                    //    AuditType = AuditType.Create;
                    //    break;

                    case EntityState.Deleted:
                        OldValues[propertyName] = property.OriginalValue;
                        break;

                        //case EntityState.Modified:
                        //    if (property.IsModified)
                        //    {
                        //        ChangedColumns.Add(dbColumnName);

                        //        OldValues[propertyName] = property.OriginalValue;
                        //        NewValues[propertyName] = property.CurrentValue;
                        //        AuditType = AuditType.Update;
                        //    }

                }
            }
            return OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues);
        }

        private void UpdateLogs(LogsLkp model)
        {
            Context.LogsLkp.Add(model);
        }
        //Internally re-usable DbSet instance.
        protected DbSet<TEntity> DbSet
        {
            get
            {
                if (_dbSet == null)
                    _dbSet = Context.Set<TEntity>();
                return _dbSet;
            }
        }
        private DbSet<TEntity> _dbSet;

        #region Regular Members
        public virtual IQueryable<TEntity> GetAll()
        {
            return DbSet.AsQueryable();
        }

        public IList<TEntity> GetAllMatched(Expression<Func<TEntity, bool>> match)
        {
            return [.. DbSet.Where(match)];
        }

        public IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> queryable = DbSet;
            foreach (Expression<Func<TEntity, object>> includeProperty in includeProperties)
            {

                queryable = queryable.Include(includeProperty);
            }
            return queryable;
        }

        //public IQueryable<TEntity> GetAllIncludingNoTracking(params Expression<Func<TEntity, object>>[] includeProperties)
        //{
        //    IQueryable<TEntity> queryable = DbSet;
        //    foreach (Expression<Func<TEntity, object>> includeProperty in includeProperties)
        //    {

        //        queryable = queryable.AsNoTracking().Include(includeProperty);
        //    }
        //    return queryable;
        //}
        private static string GetNestedPropertyName(Expression expression)
        {
            if (expression is LambdaExpression lambdaExpression)
            {
                return GetNestedPropertyName(lambdaExpression.Body);
            }
            else if (expression is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            else if (expression is UnaryExpression unaryExpression)
            {
                return GetNestedPropertyName(unaryExpression.Operand);
            }
            else if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.Name == "Select")
                {
                    // Extract the nested property from the Select expression
                    var lambda = (LambdaExpression)((UnaryExpression)methodCallExpression.Arguments[1]).Operand;
                    return GetNestedPropertyName(lambda.Body);
                }
            }

            throw new NotSupportedException("Unsupported expression type.");
        }
        public IQueryable<TEntity> GetAllIncludingNoTracking(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> queryable = DbSet.AsNoTracking();

            foreach (var includeProperty in includeProperties)
            {
                if (includeProperty.Body is MemberExpression memberExpression)
                {
                    // Handle simple Include
                    queryable = queryable.Include(includeProperty);
                }
                else if (includeProperty.Body is MethodCallExpression methodCallExpression)
                {
                    // Handle Select for nested properties
                    if (methodCallExpression.Method.Name == "Select")
                    {
                        // Extract the parent property name
                        var parentProperty = ((MemberExpression)methodCallExpression.Arguments[0]).Member.Name;

                        // Extract the nested property name
                        var nestedPropertyName = GetNestedPropertyName(methodCallExpression.Arguments[1]);

                        // Build the include path
                        queryable = queryable.Include($"{parentProperty}.{nestedPropertyName}");
                    }
                }
            }

            return queryable;
        }
        public virtual TEntity GetById(object id)
        {
            return DbSet.Find(id);
        }

        public virtual TEntity Find(Expression<Func<TEntity, bool>> match)
        {
            return DbSet.SingleOrDefault(match);
        }

        public virtual IQueryable<TEntity> GetIQueryable()
        {
            return DbSet.AsQueryable<TEntity>();
        }

        public virtual IList<TEntity> GetAllPaged(int pageIndex, int pageSize, out int totalCount)
        {
            totalCount = DbSet.Count();
            return DbSet.Skip(pageSize * pageIndex).Take(pageSize).ToList();
        }

        public int Count()
        {
            return DbSet.Count();
        }
        private void SaveChanges()
        {
            AddTimestamps();
            Context.SaveChanges();
        }
        private async Task SaveChangesAsync()
        {
            AddTimestamps();
            await Context.SaveChangesAsync();
        }
        public virtual EntityEntry<TEntity> Insert(TEntity entity, bool saveChanges = false)
        {
            var rtn = DbSet.Add(entity);
            if (saveChanges)
            {
                SaveChanges();
            }
            return rtn;
        }
        public virtual void InsertAll(List<TEntity> entity, bool saveChanges = false)
        {
            DbSet.AddRange(entity);
            if (saveChanges)
            {
                SaveChanges();
            }
        } 
         
        public virtual void Delete(object id, bool saveChanges = false)
        {
            var entity = GetById(id);
            RemoveEntity(entity);
            if (saveChanges)
            {
                SaveChanges();
            }
        }

        public virtual void Delete(TEntity entity, bool saveChanges = false)
        {
            DbSet.Attach(entity);
            RemoveEntity(entity);
            if (saveChanges)
            {
                SaveChanges();
            }
        }
        private void RemoveEntity(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public virtual void Update(TEntity entity, bool saveChanges = false)
        {
            Context.ChangeTracker.Clear();
            var entry = Context.Entry(entity);
            DbSet.Attach(entity);
            entry.State = EntityState.Modified;
            if (saveChanges)
            {
                SaveChanges();
            }
        }

        public virtual TEntity Update(TEntity entity, object key, bool saveChanges = false)
        {
            if (entity == null)
                return null;
            var exist = DbSet.Find(key);
            if (exist != null)
            {
                Context.Entry(exist).CurrentValues.SetValues(entity);
                if (saveChanges)
                {
                    SaveChanges();
                }
            }
            return exist;
        }
        public virtual void UpdateAll(IEnumerable<TEntity> entities, bool saveChanges = false)
        {
            Context.ChangeTracker.Clear();

            foreach (var entity in entities)
            {
                var entry = Context.Entry(entity);
                DbSet.Attach(entity);
                entry.State = EntityState.Modified;
            }

            if (saveChanges)
            {
                SaveChanges();
            }
        }
        public virtual void Commit()
        {
            SaveChanges();
        }
        #endregion

        #region Async Members
        public virtual async Task<IList<TEntity>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public virtual async Task<IList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> match)
        {
            return await DbSet.Where(match).ToListAsync();
        }

        public virtual async Task<TEntity> GetByIdAsync(object id)
        {
            return await DbSet.FindAsync(id);
        }
        public virtual async Task<TEntity> FindAsyncNoTracking(Expression<Func<TEntity, bool>> match)
        {
            return await DbSet.AsNoTracking().FirstOrDefaultAsync(match);
        }

        public virtual async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> match)
        {
            return await DbSet.FirstOrDefaultAsync(match);
        }

        public async Task<int> CountAsync()
        {
            return await DbSet.CountAsync();
        }

        public virtual async Task<TEntity> InsertAsync(TEntity entity, bool saveChanges = false)
        {
            var rtn = await DbSet.AddAsync(entity);
            if (saveChanges)
            {
                ////Debugging use.
                //try
                //{
                await SaveChangesAsync();
                //}
                //catch (Exception ex)
                //{
                //    var te = ex;
                //}
            }
            return rtn.Entity;
        }

        public virtual async Task DeleteAsync(object id, bool saveChanges = false)
        {
            TEntity entity = GetById(id);
            DbSet.Update(entity);
            if (saveChanges)
            {
                await SaveChangesAsync();
            }
        }

        public virtual async Task DeleteAsync(TEntity entity, bool saveChanges = false)
        {
            DbSet.Attach(entity);
            RemoveEntity(entity);
            if (saveChanges)
            {
                await SaveChangesAsync();
            }
        }

        public virtual async Task UpdateAsync(TEntity entity, bool saveChanges = false)
        {
            var entry = Context.Entry(entity);
            DbSet.Attach(entity);
            entry.State = EntityState.Modified;
            if (saveChanges)
            {
                await SaveChangesAsync();
            }
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity, object key, bool saveChanges = false)
        {
            if (entity == null)
                return null;
            var exist = await DbSet.FindAsync(key);
            if (exist != null)
            {
                Context.Entry(exist).CurrentValues.SetValues(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync();
                }
            }
            return exist;
        }

        public virtual async Task CommitAsync()
        {
            await SaveChangesAsync();
        }
        #endregion

        private bool disposed = false;
      

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
