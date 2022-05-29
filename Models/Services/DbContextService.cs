using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HRwflow.Models.Data;

namespace HRwflow.Models
{
    public class DbContextService<TPrimaryKey, TEntity>
        : IStorageService<TPrimaryKey, TEntity>
        where TEntity : class
    {
        private readonly IDatabaseContext<TEntity> _databaseContext;
        private readonly ItemLocker<object> _locker = new();

        public DbContextService(
            IDatabaseContext<TEntity> databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public void Delete(TPrimaryKey key)
        {
            using var _ = _locker.Acquire(key);
            _databaseContext.Items.Remove(
                _databaseContext.Items.Find(key));
            _databaseContext.SaveChangesAsync().Wait();
        }

        public bool TryFind(TPrimaryKey key, out TEntity entity)
        {
            entity = _databaseContext.Items.Find(key);
            return entity is not null;
        }

        public TEntity Find(TPrimaryKey key)
        {
            if (!TryFind(key, out var entity))
            {
                throw new InvalidOperationException(
                    $"Key {key} not found.");
            }
            return entity;
        }

        public bool HasKey(TPrimaryKey key)
        {
            return TryFind(key, out _);
        }

        public bool TryInsert(
            TEntity entity, out TPrimaryKey key)
        {
            key = GetPrimaryKey(entity);
            using var _ = _locker.Acquire(key);
            if (HasKey(key))
            {
                return false;
            }
            var entry = _databaseContext.Items.Add(entity);
            _databaseContext.SaveChangesAsync().Wait();
            key = GetPrimaryKey(entry.Entity);
            return true;
        }

        public TPrimaryKey Insert(TEntity entity)
        {
            if (!TryInsert(entity, out var key))
            {
                throw new InvalidOperationException(
                    $"The entity with the same primary key" +
                    $" alredy exists.");
            }
            return key;
        }

        public void Insert(TPrimaryKey key, TEntity entity)
        {
            if (!TryInsert(key, entity))
            {
                throw new InvalidOperationException(
                    $"The entity with the same primary key" +
                    $" alredy exists.");
            }
        }

        public bool TryInsert(TPrimaryKey key, TEntity entity)
        {
            if (!key.Equals(GetPrimaryKey(entity)))
            {
                throw new InvalidOperationException(
                    $"Key value is different from" +
                    $" entity primary key value.");
            }
            using var _ = _locker.Acquire(key);
            if (HasKey(key))
            {
                return false;
            }
            _databaseContext.Items.Add(entity);
            _databaseContext.SaveChangesAsync().Wait();
            return true;
        }

        public IEnumerable<TEntity> Select(
            Expression<Func<TEntity, bool>> selector)
        {
            return _databaseContext.Items.
                Where(selector).AsEnumerable();
        }

        public bool TryUpdate(TPrimaryKey key, TEntity entity)
        {
            using var _ = _locker.Acquire(key);
            if (!HasKey(key))
            {
                return false;
            }
            _databaseContext.Items.Update(entity);
            _databaseContext.SaveChangesAsync().Wait();
            return true;
        }

        public void Update(TPrimaryKey key, TEntity entity)
        {
            if (!TryUpdate(key, entity))
            {
                throw new InvalidOperationException(
                    $"Key {key} not found.");
            }
            using var _ = _locker.Acquire(key);
            _databaseContext.Items.Update(entity);
            _databaseContext.SaveChangesAsync().Wait();
        }

        private TPrimaryKey GetPrimaryKey(TEntity entity)
        {
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
