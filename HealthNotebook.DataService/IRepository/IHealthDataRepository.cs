using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthNotebook.Entities.DbSet;

namespace HealthNotebook.DataService.IRepository
{
  public interface IHealthDataRepository : IGenericRepository<HealthData>
  {
    Task<bool> UpdateHealthData(HealthData healthData);
  }
}