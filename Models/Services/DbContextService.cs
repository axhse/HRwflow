using System.Linq;
using System.Threading.Tasks;
using HRwflow.Models.Data;

namespace HRwflow.Models
{
    public class DbContextService<TPrimaryKey, TEntity> : IStorageService<TPrimaryKey, TEntity>
        where TEntity : class
    {
        private readonly IDatabaseContext<TEntity> _databaseContext;
        private readonly ItemLocker<object> _locker = new();

        public DbContextService(IDatabaseContext<TEntity> databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task<TaskResult> Delete(TPrimaryKey key)
        {
            var certificate = _locker.Acquire(key);
            try
            {
                var entity = _databaseContext.Items.Find(key);
                if (entity is null)
                {
                    return Task.FromResult(TaskResult.Unsuccessful());
                }
                _databaseContext.Items.Remove(entity);
                _databaseContext.SaveChangesAsync().Wait();
                return Task.FromResult(TaskResult.Successful());
            }
            catch
            {
                return Task.FromResult(TaskResult.Uncompleted());
            }
            finally
            {
                certificate.ReportRelease();
            }
        }

        public Task<TaskResult<TEntity>> Get(TPrimaryKey key)
        {
            var certificate = _locker.Acquire(key);
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
            finally
            {
                certificate.ReportRelease();
            }
        }

        public Task<TaskResult<TPrimaryKey>> Insert(TEntity entity)
        {
            var key = GetPrimaryKey(entity);
            var certificate = _locker.Acquire(key);
            try
            {
                if (_databaseContext.Items.Find(key) is null)
                {
                    _databaseContext.Items.Add(entity);
                    _databaseContext.SaveChangesAsync().Wait();
                    return Task.FromResult(TaskResult.Successful(key));
                }
                return Task.FromResult(TaskResult.Unsuccessful<TPrimaryKey>());
            }
            catch
            {
                return Task.FromResult(TaskResult.Uncompleted<TPrimaryKey>());
            }
            finally
            {
                certificate.ReportRelease();
            }
        }

        public Task<TaskResult> Insert(TPrimaryKey key, TEntity entity)
        {
            var certificate = _locker.Acquire(key);
            try
            {
                if (!key.Equals(GetPrimaryKey(entity)))
                {
                    return Task.FromResult(TaskResult.Uncompleted());
                }
                if (_databaseContext.Items.Find(key) is null)
                {
                    _databaseContext.Items.Add(entity);
                    _databaseContext.SaveChangesAsync().Wait();
                    return Task.FromResult(TaskResult.Successful());
                }
                return Task.FromResult(TaskResult.Unsuccessful());
            }
            catch
            {
                return Task.FromResult(TaskResult.Uncompleted());
            }
            finally
            {
                certificate.ReportRelease();
            }
        }

        public Task<TaskResult> Update(TPrimaryKey key, TEntity entity)
        {
            var certificate = _locker.Acquire(key);
            try
            {
                if (!key.Equals(GetPrimaryKey(entity)))
                {
                    return Task.FromResult(TaskResult.Uncompleted());
                }
                _databaseContext.Items.Update(entity);
                _databaseContext.SaveChangesAsync().Wait();
                return Task.FromResult(TaskResult.Successful());
            }
            catch
            {
                return Task.FromResult(TaskResult.Uncompleted());
            }
            finally
            {
                certificate.ReportRelease();
            }
        }

        private TPrimaryKey GetPrimaryKey(TEntity entity)
        {
            if (entity is null)
            {
                return default;
            }
            var entry = _databaseContext.Entry(entity);
            object[] keys = entry.Metadata.FindPrimaryKey().Properties
                         .Select(p => entry.Property(p.Name).CurrentValue)
                         .ToArray();
            if (keys.Length == 0 || keys[0] is not TPrimaryKey)
            {
                return default;
            }
            return (TPrimaryKey)keys[0];
        }
    }
}
