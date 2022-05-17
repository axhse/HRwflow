using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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

        public TaskResult Delete(TPrimaryKey key)
        {
            if (key is null)
            {
                return TaskResult.Uncompleted();
            }
            using var _ = _locker.Acquire(key);
            try
            {
                var entity = _databaseContext.Items.Find(key);
                if (entity is null)
                {
                    return TaskResult.Completed();
                }
                _databaseContext.Items.Remove(entity);
                _databaseContext.SaveChangesAsync().Wait();
                return TaskResult.Completed();
            }
            catch
            {
                return TaskResult.Uncompleted();
            }
        }

        public TaskResult<TEntity> Get(TPrimaryKey key)
        {
            if (key is null)
            {
                return TaskResult<TEntity>.Uncompleted();
            }
            // using var _ = _locker.Acquire(key);
            try
            {
                var entity = _databaseContext.Items.Find(key);
                if (entity is null)
                {
                    return TaskResult<TEntity>.Uncompleted();
                }
                return TaskResult.FromValue(entity);
            }
            catch
            {
                return TaskResult<TEntity>.Uncompleted();
            }
        }

        public TaskResult<bool> HasKey(TPrimaryKey key)
        {
            if (key is null)
            {
                return TaskResult<bool>.Uncompleted();
            }
            using var _ = _locker.Acquire(key);
            try
            {
                return TaskResult.FromValue(
                    _databaseContext.Items.Find(key) is not null);
            }
            catch
            {
                return TaskResult<bool>.Uncompleted();
            }
        }

        public TaskResult<TPrimaryKey> Insert(TEntity entity)
        {
            if (entity is null)
            {
                return TaskResult<TPrimaryKey>.Uncompleted();
            }
            var key = GetPrimaryKey(entity);
            using var _ = _locker.Acquire(key);
            try
            {
                if (_databaseContext.Items.Find(key) is not null)
                {
                    return TaskResult<TPrimaryKey>.Uncompleted();
                }
                var entry = _databaseContext.Items.Add(entity);
                _databaseContext.SaveChangesAsync().Wait();
                var generatedKey = GetPrimaryKey(entry.Entity);
                return TaskResult.FromValue(generatedKey);
            }
            catch
            {
                return TaskResult<TPrimaryKey>.Uncompleted();
            }
        }

        public TaskResult Insert(TPrimaryKey key, TEntity entity)
        {
            if (key is null || entity is null)
            {
                return TaskResult.Uncompleted();
            }
            using var _ = _locker.Acquire(key);
            try
            {
                if (key.Equals(GetPrimaryKey(entity))
                    && _databaseContext.Items.Find(key) is null)
                {
                    _databaseContext.Items.Add(entity);
                    _databaseContext.SaveChangesAsync().Wait();
                    return TaskResult.Completed();
                }
                return TaskResult.Uncompleted();
            }
            catch
            {
                return TaskResult.Uncompleted();
            }
        }

        public TaskResult<IEnumerable<TEntity>> Select(
            Expression<Func<TEntity, bool>> selector)
        {
            if (selector is null)
            {
                return TaskResult<IEnumerable<TEntity>>.Uncompleted();
            }
            try
            {
                return TaskResult.FromValue(
                    _databaseContext.Items.Where(selector).AsEnumerable());
            }
            catch
            {
                return TaskResult<IEnumerable<TEntity>>.Uncompleted();
            }
        }

        public TaskResult Update(TPrimaryKey key, TEntity entity)
        {
            if (key is null || entity is null)
            {
                return TaskResult.Uncompleted();
            }
            using var _ = _locker.Acquire(key);
            try
            {
                if (!key.Equals(GetPrimaryKey(entity)))
                {
                    return TaskResult.Uncompleted();
                }
                _databaseContext.Items.Update(entity);
                _databaseContext.SaveChangesAsync().Wait();
                return TaskResult.Completed();
            }
            catch
            {
                return TaskResult.Uncompleted();
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
