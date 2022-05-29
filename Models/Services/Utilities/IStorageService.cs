using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HRwflow.Models
{
    public interface IStorageService<Tkey, TValue>
    {
        void Delete(Tkey key);

        bool TryFind(Tkey key, out TValue value);

        TValue Find(Tkey key);

        bool HasKey(Tkey key);

        bool TryInsert(Tkey key, TValue value);

        void Insert(Tkey key, TValue value);

        bool TryInsert(TValue value, out Tkey key);

        Tkey Insert(TValue value);

        IEnumerable<TValue> Select(
            Expression<Func<TValue, bool>> selector);

        bool TryUpdate(Tkey key, TValue value);

        void Update(Tkey key, TValue value);
    }
}
