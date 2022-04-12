using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthNotebook.DataService.IRepository
{
  public interface IGenericRepository<T> where T : class
  {
    // Get all entities
    Task<IEnumerable<T>> All();
    // Get entity by Id
    Task<T> GetById(Guid Id);
    Task<bool> Add(T entity);
    Task<bool> Delete(Guid Id, string userId);
    // Update entity or add if it does not exist
    Task<bool> Upsert(T entity);
  }
}