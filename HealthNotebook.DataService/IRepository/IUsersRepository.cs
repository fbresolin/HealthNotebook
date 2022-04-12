using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthNotebook.Entities.DbSet;

namespace HealthNotebook.DataService.IRepository
{
  public interface IUsersRepository : IGenericRepository<User>
  {
  }
}