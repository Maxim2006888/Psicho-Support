using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Psicho_Support.Services.Interfaces
{
    public interface IDataService<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(int userId);
        Task<T> GetByIdAsync(int id);
        Task<T> SaveAsync(T entity);
        Task DeleteAsync(int id);
    }
}
