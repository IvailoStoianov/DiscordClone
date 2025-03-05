using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordClone.Data.Repository.Interfaces
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IRepository<T> where T : class
    {

        Task<IEnumerable<T>> GetAllAsync();
        
        Task<T?> GetByIdAsync(Guid id);
        
        Task AddAsync(T entity);
        
        Task UpdateAsync(T entity);

        Task DeleteAsync(Guid id);
        
        Task<bool> SoftDeleteAsync(Guid id);
    }
}
