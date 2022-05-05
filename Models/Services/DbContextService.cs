using System.Linq;
using System.Threading.Tasks;
using HRwflow.Models.Data;
using HRwflow.Models.Services;

namespace HRwflow.Models
{
    public class DbContextService<TPrimaryKey, TEntity> : IStorageService<TPrimaryKey, TEntity>
        where TEntity : class
    {
        private readonly IDatabaseContext<TEntity> _databaseContext;
        private readonly object _syncRoot = new();

        public DbContextService(IDatabaseContext<TEntity> databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task<TaskResult> Delete(TPrimaryKey key)
        {
            lock (_syncRoot)
            {
                try
                {
                    var entity = _databaseContext.Items.Find(key);
                    if (entity is null)
                    {
                        return Task.FromResult(TaskResult.Unsuccessful());
                    }
                    _databaseContext.Items.Remove(entity);
                    _databaseContext.SaveChangesAsync();
                    return Task.FromResult(TaskResult.Successful());
                }
                catch
                {
                    return Task.FromResult(TaskResult.Uncompleted());
                }
            }
        }

        public Task<TaskResult<TEntity>> Get(TPrimaryKey key)
        {
            lock (_syncRoot)
            {
                try
                {
                    var entity = _databaseContext.Items.Find(key);
                    if (entity is null)
                    {
                        return Task.FromResult(TaskResult.Unsuccessful<TEntity>());
                    }
                    return Task.FromResult(TaskResult.Successful(entity));
                }
                catch
                {
                    return Task.FromResult(TaskResult.Uncompleted<TEntity>());
                }
            }
        }

        public Task<TaskResult<TPrimaryKey>> Insert(TEntity entity)
        {
            if (!TryGetPrimaryKey(entity, out TPrimaryKey entityKey))
            {
                return Task.FromResult(TaskResult.Uncompleted<TPrimaryKey>());
            }
            lock (_syncRoot)
            {
                try
                {
                    if (_databaseContext.Items.Find(entityKey) is null)
                    {
                        _databaseContext.Items.Add(entity);
                        _databaseContext.SaveChangesAsync();
                        return Task.FromResult(TaskResult.Successful(entityKey));
                    }
                    return Task.FromResult(TaskResult.Unsuccessful<TPrimaryKey>());
                }
                catch
                {
                    return Task.FromResult(TaskResult.Uncompleted<TPrimaryKey>());
                }
            }
        }

        public Task<TaskResult> Insert(TPrimaryKey key, TEntity entity)
        {
            if (!TryGetPrimaryKey(entity, out TPrimaryKey entityKey) || !entityKey.Equals(key))
            {
                return Task.FromResult(TaskResult.Uncompleted());
            }
            lock (_syncRoot)
            {
                try
                {
                    if (_databaseContext.Items.Find(entityKey) is null)
                    {
                        _databaseContext.Items.Add(entity);
                        _databaseContext.SaveChangesAsync();
                        return Task.FromResult(TaskResult.Successful());
                    }
                    return Task.FromResult(TaskResult.Unsuccessful());
                }
                catch
                {
                    return Task.FromResult(TaskResult.Uncompleted());
                }
            }
        }

        public Task<TaskResult> Update(TPrimaryKey key, TEntity entity)
        {
            if (!TryGetPrimaryKey(entity, out TPrimaryKey entityKey) || !entityKey.Equals(key))
            {
                return Task.FromResult(TaskResult.Uncompleted());
            }
            lock (_syncRoot)
            {
                try
                {
                    _databaseContext.Items.Update(entity);
                    _databaseContext.SaveChangesAsync();
                    return Task.FromResult(TaskResult.Successful());
                }
                catch
                {
                    return Task.FromResult(TaskResult.Uncompleted());
                }
            }
        }

        private bool TryGetPrimaryKey(TEntity entity, out TPrimaryKey key)
        {
            if (entity is null)
            {
                key = default;
                return false;
            }
            lock (_syncRoot)
            {
                var entry = _databaseContext.Entry(entity);
                object[] keys = entry.Metadata.FindPrimaryKey().Properties
                             .Select(p => entry.Property(p.Name).CurrentValue)
                             .ToArray();
                if (keys.Length == 0 || keys[0] is not TPrimaryKey)
                {
                    key = default;
                    return false;
                }
                key = (TPrimaryKey)keys[0];
                return true;
            }
        }
    }
}
