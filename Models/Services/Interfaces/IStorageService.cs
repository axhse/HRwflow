using System.Threading.Tasks;

namespace HRwflow.Models.Services
{
    public interface IStorageService<Tkey, TValue>

    {
        Task<TaskResult> Delete(Tkey key);

        Task<TaskResult<TValue>> Get(Tkey key);

        Task<TaskResult> Insert(Tkey key, TValue value);

        Task<TaskResult<Tkey>> Insert(TValue value);

        Task<TaskResult> Update(Tkey key, TValue value);
    }
}
