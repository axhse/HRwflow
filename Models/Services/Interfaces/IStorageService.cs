namespace HRwflow.Models
{
    public interface IStorageService<Tkey, TValue>
    {
        TaskResult Delete(Tkey key);

        TaskResult<TValue> Get(Tkey key);

        TaskResult<bool> HasKey(Tkey key);

        TaskResult Insert(Tkey key, TValue value);

        TaskResult<Tkey> Insert(TValue value);

        TaskResult Update(Tkey key, TValue value);
    }
}
