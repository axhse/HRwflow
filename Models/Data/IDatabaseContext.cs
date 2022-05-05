using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HRwflow.Models.Data
{
    public interface IDatabaseContext<TEntity> where TEntity : class
    {
        DbSet<TEntity> Items { get; set; }

        EntityEntry<TEntity> Entry(TEntity entity);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
