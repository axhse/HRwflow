using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HRwflow.Models
{
    public interface IStorageService<Tkey, TValue>
    {
        TaskResult Delete(Tkey key);

        TaskResult<TValue> Get(Tkey key);

        TaskResult<bool> HasKey(Tkey key);

        TaskResult Insert(Tkey key, TValue value);

        TaskResult<Tkey> Insert(TValue value);

        TaskResult<IEnumerable<TValue>> Select(
            Expression<Func<TValue, bool>> selector);

        TaskResult Update(Tkey key, TValue value);
    }
}
